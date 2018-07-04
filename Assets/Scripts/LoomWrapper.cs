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

	public string username;
	public Contract contract;

	public delegate void Callback<T> (T result);

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

	public async Task CreateAccount() {
		LDCreateAccountTx tx = new LDCreateAccountTx ();
		tx.Owner = username;
		try {
			await contract.CallAsync ("CreateAccount", tx);
		} catch {
			// ignore for now
		}
	}

	public async Task GetChipCount(Callback<LDChipQueryResult> callback)
	{
		LDChipQueryParams cq = new LDChipQueryParams ();
		cq.Owner = username;

		LDChipQueryResult result  = await contract.StaticCallAsync<LDChipQueryResult>("GetChipCount", cq);
		if (callback != null) {
			callback (result);
		}
	}

	public async Task RollDice(int amount, bool betBig, Callback<LDRollQueryResult> callback) {
		LDRollQueryParams rq = new LDRollQueryParams ();
		rq.Owner = username;
		rq.Amount = amount;
		rq.BetBig = betBig;

		LDRollQueryResult result = await contract.CallAsync<LDRollQueryResult> ("Roll", rq);
		if (callback != null) {
			callback (result);
		}
	}
}

