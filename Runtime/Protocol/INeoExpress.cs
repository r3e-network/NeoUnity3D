using System.Collections.Generic;
using System.Threading.Tasks;
using NeoUnity.Runtime.Protocol.Response;
using NeoUnity.Runtime.Types;

namespace NeoUnity.Runtime.Protocol
{
    /// <summary>
    /// Interface for Neo Express development features.
    /// Neo Express is a developer-oriented blockchain for testing and development.
    /// </summary>
    public interface INeoExpress
    {
        /// <summary>
        /// Gets the populated blocks information from Neo Express.
        /// Returns statistics about blocks that contain transactions.
        /// </summary>
        /// <returns>Populated blocks information</returns>
        Task<NeoResponse<PopulatedBlocks>> ExpressGetPopulatedBlocks();

        /// <summary>
        /// Gets all NEP-17 contracts deployed on Neo Express.
        /// Useful for discovering available tokens in development environment.
        /// </summary>
        /// <returns>List of NEP-17 contracts</returns>
        Task<NeoResponse<List<Nep17Contract>>> ExpressGetNep17Contracts();

        /// <summary>
        /// Gets the storage entries for a specific contract on Neo Express.
        /// Allows inspection of contract state for debugging purposes.
        /// </summary>
        /// <param name="contractHash">The contract hash to get storage for</param>
        /// <returns>List of contract storage entries</returns>
        Task<NeoResponse<List<ContractStorageEntry>>> ExpressGetContractStorage(Hash160 contractHash);

        /// <summary>
        /// Lists all contracts deployed on Neo Express.
        /// Includes both native and deployed contracts with their states.
        /// </summary>
        /// <returns>List of contract states</returns>
        Task<NeoResponse<List<ExpressContractState>>> ExpressListContracts();

        /// <summary>
        /// Creates a checkpoint file for Neo Express blockchain state.
        /// Allows saving the current blockchain state to a file for later restoration.
        /// </summary>
        /// <param name="filename">The filename for the checkpoint</param>
        /// <returns>Result message</returns>
        Task<NeoResponse<string>> ExpressCreateCheckpoint(string filename);

        /// <summary>
        /// Lists all pending oracle requests on Neo Express.
        /// Useful for development and testing of oracle functionality.
        /// </summary>
        /// <returns>List of oracle requests</returns>
        Task<NeoResponse<List<OracleRequest>>> ExpressListOracleRequests();

        /// <summary>
        /// Creates an oracle response transaction on Neo Express.
        /// Allows manual creation of oracle responses for testing.
        /// </summary>
        /// <param name="oracleResponse">The oracle response transaction attribute</param>
        /// <returns>Transaction hash</returns>
        Task<NeoResponse<string>> ExpressCreateOracleResponseTx(TransactionAttribute oracleResponse);

        /// <summary>
        /// Shuts down the Neo Express blockchain instance.
        /// Gracefully stops the Neo Express node.
        /// </summary>
        /// <returns>Shutdown information</returns>
        Task<NeoResponse<ExpressShutdown>> ExpressShutdown();
    }
}