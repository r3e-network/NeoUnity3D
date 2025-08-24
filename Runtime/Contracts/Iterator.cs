using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Neo.Unity.SDK.Core;
using Neo.Unity.SDK.Types;

namespace Neo.Unity.SDK.Contracts
{
    /// <summary>
    /// Represents an iterator for traversing large data sets returned by smart contracts.
    /// Provides Unity-optimized async iteration over blockchain data with session management.
    /// </summary>
    /// <typeparam name="T">The type to map stack items to</typeparam>
    [System.Serializable]
    public class Iterator<T> : IDisposable
    {
        #region Private Fields
        
        private readonly NeoUnity neoUnity;
        private readonly string sessionId;
        private readonly string iteratorId;
        private readonly Func<StackItem, T> mapper;
        private bool disposed = false;
        private bool sessionTerminated = false;
        
        #endregion
        
        #region Properties
        
        /// <summary>The session ID for this iterator</summary>
        public string SessionId => sessionId;
        
        /// <summary>The iterator ID within the session</summary>
        public string IteratorId => iteratorId;
        
        /// <summary>Whether the iterator session has been terminated</summary>
        public bool IsSessionTerminated => sessionTerminated;
        
        /// <summary>Whether the iterator has been disposed</summary>
        public bool IsDisposed => disposed;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Creates a new iterator with the specified parameters.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance for RPC calls</param>
        /// <param name="sessionId">The session ID</param>
        /// <param name="iteratorId">The iterator ID</param>
        /// <param name="mapper">Function to map stack items to desired type</param>
        public Iterator(NeoUnity neoUnity, string sessionId, string iteratorId, Func<StackItem, T> mapper)
        {
            this.neoUnity = neoUnity ?? throw new ArgumentNullException(nameof(neoUnity));
            this.sessionId = sessionId ?? throw new ArgumentNullException(nameof(sessionId));
            this.iteratorId = iteratorId ?? throw new ArgumentNullException(nameof(iteratorId));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        
        #endregion
        
        #region Iterator Operations
        
        /// <summary>
        /// Traverses the iterator and returns up to the specified number of items.
        /// </summary>
        /// <param name="count">Maximum number of items to retrieve</param>
        /// <returns>List of mapped items</returns>
        /// <exception cref="InvalidOperationException">If iterator is disposed or session terminated</exception>
        public async Task<List<T>> Traverse(int count = 100)
        {
            EnsureNotDisposed();
            EnsureSessionActive();
            
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");
            
            try
            {
                var response = await neoUnity.TraverseIterator(sessionId, iteratorId, count).SendAsync();
                var stackItems = response.GetResult();
                
                var results = new List<T>(stackItems.Count);
                
                foreach (var stackItem in stackItems)
                {
                    try
                    {
                        var mappedItem = mapper(stackItem);
                        results.Add(mappedItem);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[Iterator] Failed to map stack item: {ex.Message}");
                        // Continue processing other items
                    }
                }
                
                if (neoUnity.Config.EnableDebugLogging)
                {
                    Debug.Log($"[Iterator] Traversed {results.Count} items from iterator {iteratorId}");
                }
                
                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to traverse iterator: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Traverses all remaining items in the iterator.
        /// WARNING: Use with caution on large data sets.
        /// </summary>
        /// <param name="batchSize">Size of each batch to process</param>
        /// <param name="maxItems">Maximum total items to retrieve (safety limit)</param>
        /// <returns>List of all mapped items</returns>
        public async Task<List<T>> TraverseAll(int batchSize = 100, int maxItems = 10000)
        {
            EnsureNotDisposed();
            EnsureSessionActive();
            
            if (batchSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(batchSize), "Batch size must be positive.");
            
            if (maxItems <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxItems), "Max items must be positive.");
            
            var allResults = new List<T>();
            var totalRetrieved = 0;
            
            try
            {
                while (totalRetrieved < maxItems)
                {
                    var remainingItems = maxItems - totalRetrieved;
                    var currentBatchSize = Math.Min(batchSize, remainingItems);
                    
                    var batchResults = await Traverse(currentBatchSize);
                    
                    if (batchResults.Count == 0)
                    {
                        // No more items available
                        break;
                    }
                    
                    allResults.AddRange(batchResults);
                    totalRetrieved += batchResults.Count;
                    
                    // If we got fewer items than requested, we've reached the end
                    if (batchResults.Count < currentBatchSize)
                    {
                        break;
                    }
                    
                    // Small delay to prevent overwhelming the node
                    await Task.Delay(10);
                }
                
                if (neoUnity.Config.EnableDebugLogging)
                {
                    Debug.Log($"[Iterator] Retrieved {allResults.Count} total items from iterator {iteratorId}");
                }
                
                return allResults;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to traverse all items: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Terminates the iterator session to free server resources.
        /// </summary>
        /// <returns>True if session was successfully terminated</returns>
        public async Task<bool> TerminateSession()
        {
            if (sessionTerminated || disposed)
                return true;
            
            try
            {
                var response = await neoUnity.TerminateSession(sessionId).SendAsync();
                var result = response.GetResult();
                
                sessionTerminated = true;
                
                if (neoUnity.Config.EnableDebugLogging)
                {
                    Debug.Log($"[Iterator] Terminated session {sessionId}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Iterator] Failed to terminate session {sessionId}: {ex.Message}");
                sessionTerminated = true; // Mark as terminated to prevent further attempts
                return false;
            }
        }
        
        #endregion
        
        #region Validation Methods
        
        /// <summary>
        /// Ensures the iterator has not been disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">If iterator is disposed</exception>
        private void EnsureNotDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(Iterator<T>));
        }
        
        /// <summary>
        /// Ensures the session is still active.
        /// </summary>
        /// <exception cref="InvalidOperationException">If session is terminated</exception>
        private void EnsureSessionActive()
        {
            if (sessionTerminated)
                throw new InvalidOperationException("Iterator session has been terminated.");
        }
        
        #endregion
        
        #region IDisposable Implementation
        
        /// <summary>
        /// Disposes the iterator and terminates its session.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                // Try to terminate session asynchronously
                if (!sessionTerminated)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await TerminateSession();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[Iterator] Failed to terminate session during disposal: {ex.Message}");
                        }
                    });
                }
                
                disposed = true;
            }
        }
        
        #endregion
        
        #region Unity Integration
        
        /// <summary>
        /// Unity-friendly method to traverse iterator with coroutine support.
        /// </summary>
        /// <param name="count">Number of items to retrieve</param>
        /// <param name="onProgress">Progress callback (currentItems, estimatedTotal)</param>
        /// <returns>List of mapped items</returns>
        public async Task<List<T>> TraverseWithProgress(int count, Action<int, int> onProgress = null)
        {
            EnsureNotDisposed();
            EnsureSessionActive();
            
            try
            {
                onProgress?.Invoke(0, count);
                
                var results = await Traverse(count);
                
                onProgress?.Invoke(results.Count, count);
                
                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to traverse with progress: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Gets a preview of the first few items without affecting the iterator state.
        /// Note: This actually consumes items from the iterator.
        /// </summary>
        /// <param name="previewCount">Number of items to preview</param>
        /// <returns>Preview items</returns>
        public async Task<List<T>> GetPreview(int previewCount = 5)
        {
            return await Traverse(Math.Max(1, previewCount));
        }
        
        #endregion
        
        #region String Representation
        
        /// <summary>
        /// Returns a string representation of this iterator.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            var status = disposed ? "Disposed" : 
                        sessionTerminated ? "Terminated" : "Active";
            
            return $"Iterator<{typeof(T).Name}>(Session: {sessionId}, Iterator: {iteratorId}, Status: {status})";
        }
        
        #endregion
    }
    
    /// <summary>
    /// Static helper methods for working with iterators.
    /// </summary>
    public static class IteratorHelpers
    {
        /// <summary>
        /// Creates a simple iterator that maps stack items to their string representation.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance</param>
        /// <param name="sessionId">The session ID</param>
        /// <param name="iteratorId">The iterator ID</param>
        /// <returns>String iterator</returns>
        public static Iterator<string> CreateStringIterator(NeoUnity neoUnity, string sessionId, string iteratorId)
        {
            return new Iterator<string>(neoUnity, sessionId, iteratorId, item => item.GetString());
        }
        
        /// <summary>
        /// Creates an iterator that maps stack items to their integer representation.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance</param>
        /// <param name="sessionId">The session ID</param>
        /// <param name="iteratorId">The iterator ID</param>
        /// <returns>Integer iterator</returns>
        public static Iterator<int> CreateIntegerIterator(NeoUnity neoUnity, string sessionId, string iteratorId)
        {
            return new Iterator<int>(neoUnity, sessionId, iteratorId, item => item.GetInteger());
        }
        
        /// <summary>
        /// Creates an iterator that maps stack items to their byte array representation.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance</param>
        /// <param name="sessionId">The session ID</param>
        /// <param name="iteratorId">The iterator ID</param>
        /// <returns>Byte array iterator</returns>
        public static Iterator<byte[]> CreateByteArrayIterator(NeoUnity neoUnity, string sessionId, string iteratorId)
        {
            return new Iterator<byte[]>(neoUnity, sessionId, iteratorId, item => item.GetByteArray());
        }
        
        /// <summary>
        /// Creates an iterator that preserves the original stack items.
        /// </summary>
        /// <param name="neoUnity">The NeoUnity instance</param>
        /// <param name="sessionId">The session ID</param>
        /// <param name="iteratorId">The iterator ID</param>
        /// <returns>StackItem iterator</returns>
        public static Iterator<StackItem> CreateStackItemIterator(NeoUnity neoUnity, string sessionId, string iteratorId)
        {
            return new Iterator<StackItem>(neoUnity, sessionId, iteratorId, item => item);
        }
    }
}