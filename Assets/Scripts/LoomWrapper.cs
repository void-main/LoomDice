using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Loom.Unity3d;

public class LoomWrapper
{
	private static readonly Lazy<LoomWrapper> lazy =
		new Lazy<LoomWrapper>(() => new LoomWrapper());

	public static LoomWrapper Instance { get { return lazy.Value; } }

	private LoomWrapper()
	{
	}

	public async Task<Contract> GetContract(byte[] privateKey, byte[] publicKey)
	{
		var writer = RPCClientFactory.Configure()
			.WithLogger(Debug.unityLogger)
			.WithHTTP("http://13.231.20.92:46658/rpc")
			.Create();

		var reader = RPCClientFactory.Configure()
			.WithLogger(Debug.unityLogger)
			.WithHTTP("http://13.231.20.92:46658/query")
			.Create();

		var client = new DAppChainClient(writer, reader)
		{
			Logger = Debug.unityLogger
		};
		// required middleware
		client.TxMiddleware = new TxMiddleware(new ITxMiddlewareHandler[]{
			new NonceTxMiddleware(
				publicKey,
				client
			),
			new SignedTxMiddleware(privateKey)
		});
		var contractAddr = await client.ResolveContractAddressAsync("loomdicecore");
		var callerAddr = Address.FromPublicKey(publicKey);
		return new Contract(client, contractAddr, callerAddr);
	}

	public async Task CreateAccount(Contract contract, string username) {
		LDCreateAccountTx tx = new LDCreateAccountTx ();
		tx.Owner = username;
		try {
			await contract.CallAsync ("CreateAccount", tx);
		} catch {
			// ignore for now
		}
	}

	public async Task GetChipCount(Contract contract, string username)
	{
		LDChipQueryParams cq = new LDChipQueryParams ();
		cq.Owner = username;

		LDChipQueryResult result  = await contract.StaticCallAsync<LDChipQueryResult>("GetChipCount", cq);

		if (result != null)
		{
			// This should print: { "key": "123", "value": "hello!" } in the Unity console window
			// provided `LoomQuickStartSample.CallContract()` was called first.
			Debug.Log("Smart contract returned: " + result.Amount);
		}
		else
		{
			throw new Exception("Smart contract didn't return anything!");
		}
	}
}

