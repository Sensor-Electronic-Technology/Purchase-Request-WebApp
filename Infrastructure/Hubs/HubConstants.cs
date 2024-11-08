namespace Infrastructure.Hubs;

public static class HubConstants {
    public static string HubUrl { get; } = "/messagehub";
    public static class Events {
        public static string ReceiveMessage { get; } ="ReceiveMessage";
        public static string ReceiveRefresh { get; } = "ReceiveRefresh";
        public static string Connected { get; } = "Connected";
    }
    public static class Methods {
        public static string SendMessage { get; } = "SendMessage";
        public static string SendRefresh { get; } = "SendRefresh";
        public static string SendRefreshAll { get; } = "SendRefreshAll";
        public static string Register { get; } = "Register";
    }
}