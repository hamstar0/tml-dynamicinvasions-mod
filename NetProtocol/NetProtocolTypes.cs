namespace DynamicInvasions.NetProtocol {
	public enum NetProtocolTypes : byte {
		RequestModSettings,
		ModSettings,
		Invasion,
		InvasionStatus,
		EndInvasion,
		RequestInvasion,
		RequestInvasionStatus,
		RequestEndInvasion
	}
}
