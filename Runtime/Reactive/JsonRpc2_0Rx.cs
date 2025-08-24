using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using NeoUnity.Runtime.Core;
using NeoUnity.Runtime.Protocol.Response;

namespace NeoUnity.Runtime.Reactive
{
    /// <summary>
    /// Reactive extensions for JSON-RPC 2.0 protocol using Unity-compatible async patterns.
    /// Provides observable-like functionality for blockchain events without external dependencies.
    /// </summary>
    public class JsonRpc2_0Rx
    {
        private readonly INeo neoUnity;
        private readonly CancellationToken defaultCancellationToken;

        /// <summary>
        /// Initializes a new instance of JsonRpc2_0Rx.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance</param>
        /// <param name="cancellationToken">Default cancellation token for operations</param>
        public JsonRpc2_0Rx(INeo neoUnity, CancellationToken cancellationToken = default)
        {
            this.neoUnity = neoUnity ?? throw new ArgumentNullException(nameof(neoUnity));
            this.defaultCancellationToken = cancellationToken;
        }

        /// <summary>
        /// Creates an async enumerable that yields block indices as they are created.
        /// </summary>
        /// <param name="pollingIntervalMs">Polling interval in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of block indices</returns>
        public async IAsyncEnumerable<int> GetBlockIndexStream(
            int pollingIntervalMs = 1000,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var token = cancellationToken == default ? defaultCancellationToken : cancellationToken;
            int lastBlockIndex = -1;

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var response = await neoUnity.GetBlockCount();
                    var currentBlockIndex = response.GetResult() - 1; // Block count is 1-based, index is 0-based

                    if (currentBlockIndex > lastBlockIndex)
                    {
                        lastBlockIndex = currentBlockIndex;
                        yield return currentBlockIndex;
                    }

                    await Task.Delay(pollingIntervalMs, token);
                }
                catch (OperationCanceledException)
                {
                    yield break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JsonRpc2_0Rx] Error polling block index: {ex.Message}");
                    await Task.Delay(pollingIntervalMs, token);
                }
            }
        }

        /// <summary>
        /// Creates an async enumerable that yields new blocks as they are created.
        /// </summary>
        /// <param name="fullTransactionObjects">Whether to include full transaction objects</param>
        /// <param name="pollingIntervalMs">Polling interval in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of blocks</returns>
        public async IAsyncEnumerable<NeoBlock> GetBlockStream(
            bool fullTransactionObjects = false,
            int pollingIntervalMs = 1000,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var token = cancellationToken == default ? defaultCancellationToken : cancellationToken;

            await foreach (var blockIndex in GetBlockIndexStream(pollingIntervalMs, token))
            {
                try
                {
                    var response = await neoUnity.GetBlock(blockIndex, fullTransactionObjects);
                    yield return response.GetResult();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JsonRpc2_0Rx] Error getting block {blockIndex}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Replays blocks within the specified range.
        /// </summary>
        /// <param name="startBlock">Start block index</param>
        /// <param name="endBlock">End block index</param>
        /// <param name="fullTransactionObjects">Whether to include full transaction objects</param>
        /// <param name="ascending">Whether to replay in ascending order</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of blocks</returns>
        public async IAsyncEnumerable<NeoBlock> ReplayBlocks(
            int startBlock,
            int endBlock,
            bool fullTransactionObjects = false,
            bool ascending = true,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var token = cancellationToken == default ? defaultCancellationToken : cancellationToken;
            
            var blocks = Enumerable.Range(startBlock, endBlock - startBlock + 1);
            if (!ascending)
            {
                blocks = blocks.Reverse();
            }

            foreach (var blockIndex in blocks)
            {
                if (token.IsCancellationRequested)
                    yield break;

                try
                {
                    var response = await neoUnity.GetBlock(blockIndex, fullTransactionObjects);
                    yield return response.GetResult();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[JsonRpc2_0Rx] Error replaying block {blockIndex}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Catches up to the latest block and then continues with new blocks.
        /// </summary>
        /// <param name="startBlock">Starting block index</param>
        /// <param name="fullTransactionObjects">Whether to include full transaction objects</param>
        /// <param name="pollingIntervalMs">Polling interval for new blocks</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of blocks</returns>
        public async IAsyncEnumerable<NeoBlock> CatchUpToLatestAndSubscribeToNewBlocks(
            int startBlock,
            bool fullTransactionObjects = false,
            int pollingIntervalMs = 1000,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var token = cancellationToken == default ? defaultCancellationToken : cancellationToken;

            try
            {
                // Get the latest block index first
                var blockCountResponse = await neoUnity.GetBlockCount();
                var latestBlock = blockCountResponse.GetResult() - 1;

                if (startBlock <= latestBlock)
                {
                    // Replay historical blocks
                    await foreach (var block in ReplayBlocks(startBlock, latestBlock, fullTransactionObjects, true, token))
                    {
                        yield return block;
                    }
                }

                // Continue with new blocks
                await foreach (var block in GetBlockStream(fullTransactionObjects, pollingIntervalMs, token))
                {
                    yield return block;
                }
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
        }

        /// <summary>
        /// Gets the latest block index.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Latest block index</returns>
        public async Task<int> GetLatestBlockIndex(CancellationToken cancellationToken = default)
        {
            var token = cancellationToken == default ? defaultCancellationToken : cancellationToken;
            var response = await neoUnity.GetBlockCount();
            return response.GetResult() - 1; // Convert count to index
        }

        /// <summary>
        /// Creates an async enumerable for transaction monitoring within blocks.
        /// </summary>
        /// <param name="transactionPredicate">Predicate to filter transactions</param>
        /// <param name="pollingIntervalMs">Polling interval in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of matching transactions</returns>
        public async IAsyncEnumerable<NeoTransaction> GetTransactionStream(
            Func<NeoTransaction, bool> transactionPredicate = null,
            int pollingIntervalMs = 1000,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var token = cancellationToken == default ? defaultCancellationToken : cancellationToken;

            await foreach (var block in GetBlockStream(true, pollingIntervalMs, token))
            {
                if (block.Transactions != null)
                {
                    foreach (var transaction in block.Transactions)
                    {
                        if (transactionPredicate == null || transactionPredicate(transaction))
                        {
                            yield return transaction;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Creates an event handler pattern for block notifications.
        /// </summary>
        public class BlockNotificationHandler
        {
            public event Action<NeoBlock> OnNewBlock;
            public event Action<Exception> OnError;
            
            private CancellationTokenSource cancellationTokenSource;

            /// <summary>
            /// Starts listening for new blocks.
            /// </summary>
            /// <param name="rx">JsonRpc2_0Rx instance</param>
            /// <param name="fullTransactionObjects">Whether to include full transaction objects</param>
            /// <param name="pollingIntervalMs">Polling interval</param>
            public void StartListening(JsonRpc2_0Rx rx, bool fullTransactionObjects = false, int pollingIntervalMs = 1000)
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        await foreach (var block in rx.GetBlockStream(fullTransactionObjects, pollingIntervalMs, cancellationTokenSource.Token))
                        {
                            OnNewBlock?.Invoke(block);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when stopping
                    }
                    catch (Exception ex)
                    {
                        OnError?.Invoke(ex);
                    }
                });
            }

            /// <summary>
            /// Stops listening for new blocks.
            /// </summary>
            public void StopListening()
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }
    }

    /// <summary>
    /// Unity MonoBehaviour wrapper for reactive blockchain operations.
    /// Provides automatic lifecycle management and Unity integration.
    /// </summary>
    public class ReactiveBlockchainMonitor : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool fullTransactionObjects = false;
        [SerializeField] private int pollingIntervalMs = 1000;
        [SerializeField] private bool startOnAwake = true;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent<NeoBlock> OnNewBlock;
        public UnityEngine.Events.UnityEvent<string> OnError;

        private JsonRpc2_0Rx.BlockNotificationHandler handler;
        private INeo neoUnity;

        /// <summary>
        /// Initialize the monitor.
        /// </summary>
        /// <param name="neoUnityInstance">NeoUnity instance to use</param>
        public void Initialize(INeo neoUnityInstance)
        {
            neoUnity = neoUnityInstance ?? throw new ArgumentNullException(nameof(neoUnityInstance));
            
            handler = new JsonRpc2_0Rx.BlockNotificationHandler();
            handler.OnNewBlock += block => OnNewBlock?.Invoke(block);
            handler.OnError += ex => OnError?.Invoke(ex.Message);
        }

        private void Awake()
        {
            if (startOnAwake && neoUnity != null)
            {
                StartMonitoring();
            }
        }

        /// <summary>
        /// Start monitoring for new blocks.
        /// </summary>
        public void StartMonitoring()
        {
            if (neoUnity == null)
            {
                Debug.LogError("[ReactiveBlockchainMonitor] NeoUnity instance not set. Call Initialize first.");
                return;
            }

            var rx = new JsonRpc2_0Rx(neoUnity);
            handler?.StartListening(rx, fullTransactionObjects, pollingIntervalMs);
        }

        /// <summary>
        /// Stop monitoring for new blocks.
        /// </summary>
        public void StopMonitoring()
        {
            handler?.StopListening();
        }

        private void OnDestroy()
        {
            StopMonitoring();
        }
    }
}