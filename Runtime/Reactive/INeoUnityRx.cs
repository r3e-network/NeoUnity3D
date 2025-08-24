using System;
using System.Collections.Generic;
using System.Threading;
using NeoUnity.Runtime.Protocol.Response;

namespace NeoUnity.Runtime.Reactive
{
    /// <summary>
    /// The Unity-compatible reactive blockchain API interface.
    /// Provides async enumerable patterns for blockchain event streams.
    /// </summary>
    public interface INeoUnityRx
    {
        /// <summary>
        /// Creates an async enumerable that emits newly created blocks on the blockchain.
        /// </summary>
        /// <param name="fullTransactionObjects">If true, provides transactions embedded in blocks, otherwise transaction hashes</param>
        /// <param name="pollingIntervalMs">Polling interval in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An async enumerable that emits all new blocks as they are added to the blockchain</returns>
        IAsyncEnumerable<NeoBlock> GetBlockStream(bool fullTransactionObjects = false, int pollingIntervalMs = 1000, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an async enumerable that emits all blocks from the blockchain contained within the requested range.
        /// </summary>
        /// <param name="startBlock">The block number to commence with</param>
        /// <param name="endBlock">The block number to finish with</param>
        /// <param name="fullTransactionObjects">If true, provides transactions embedded in blocks, otherwise transaction hashes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An async enumerable to emit these blocks</returns>
        IAsyncEnumerable<NeoBlock> ReplayBlocks(int startBlock, int endBlock, bool fullTransactionObjects = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an async enumerable that emits all blocks from the blockchain contained within the requested range.
        /// </summary>
        /// <param name="startBlock">The block number to commence with</param>
        /// <param name="endBlock">The block number to finish with</param>
        /// <param name="fullTransactionObjects">If true, provides transactions embedded in blocks, otherwise transaction hashes</param>
        /// <param name="ascending">If true, emits blocks in ascending order between range, otherwise, in descending order</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An async enumerable to emit these blocks</returns>
        IAsyncEnumerable<NeoBlock> ReplayBlocks(int startBlock, int endBlock, bool fullTransactionObjects, bool ascending, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an async enumerable that emits all blocks from the blockchain starting with a provided block number.
        /// Once it has replayed up to the most current block, the enumerable completes.
        /// </summary>
        /// <param name="startBlock">The block number to commence with</param>
        /// <param name="fullTransactionObjects">If true, provides transactions embedded in blocks, otherwise transaction hashes</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An async enumerable to emit all requested blocks</returns>
        IAsyncEnumerable<NeoBlock> CatchUpToLatestBlocks(int startBlock, bool fullTransactionObjects = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an async enumerable that emits all blocks from the requested block number to the most current.
        /// Once it has emitted the most current block, it starts emitting new blocks as they are created.
        /// </summary>
        /// <param name="startBlock">The block number to commence with</param>
        /// <param name="fullTransactionObjects">If true, provides transactions embedded in blocks, otherwise transaction hashes</param>
        /// <param name="pollingIntervalMs">Polling interval for new blocks in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An async enumerable to emit all requested blocks and future blocks</returns>
        IAsyncEnumerable<NeoBlock> CatchUpToLatestAndSubscribeToNewBlocks(int startBlock, bool fullTransactionObjects = false, int pollingIntervalMs = 1000, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an async enumerable that emits new blocks as they are created on the blockchain (starting from the latest block).
        /// </summary>
        /// <param name="fullTransactionObjects">If true, provides transactions embedded in blocks, otherwise transaction hashes</param>
        /// <param name="pollingIntervalMs">Polling interval in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An async enumerable to emit all future blocks</returns>
        IAsyncEnumerable<NeoBlock> SubscribeToNewBlocks(bool fullTransactionObjects = false, int pollingIntervalMs = 1000, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an async enumerable that emits transactions matching the provided predicate.
        /// </summary>
        /// <param name="transactionPredicate">Predicate to filter transactions (null to include all)</param>
        /// <param name="pollingIntervalMs">Polling interval in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An async enumerable of matching transactions</returns>
        IAsyncEnumerable<NeoTransaction> GetTransactionStream(Func<NeoTransaction, bool> transactionPredicate = null, int pollingIntervalMs = 1000, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an async enumerable that emits block indices as they are created.
        /// </summary>
        /// <param name="pollingIntervalMs">Polling interval in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An async enumerable of block indices</returns>
        IAsyncEnumerable<int> GetBlockIndexStream(int pollingIntervalMs = 1000, CancellationToken cancellationToken = default);
    }
}