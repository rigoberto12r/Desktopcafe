namespace Desktopcafe.Shared;

public static class Constants
{
    public const string HubPath = "/cafehub";
    public const int ServerPort = 5000;
    public const int HeartbeatIntervalSeconds = 30;
    public const int ClientTimeoutSeconds = 60;

    public static class Defaults
    {
        public const decimal PricePerHour = 20.00m;
        public const decimal PricePerHalfHour = 12.00m;
        public const decimal PricePer15Min = 7.00m;
        public const decimal PrintPriceBW = 2.00m;
        public const decimal PrintPriceColor = 5.00m;
        public const int WarningMinutes1 = 5;
        public const int WarningMinutes2 = 2;
        public const int WarningMinutes3 = 1;
        public const string CafeName = "Desktopcafe";
        public const string LockScreenMessage = "Esta computadora esta bloqueada. Solicite acceso en el mostrador.";
    }

    public static class ConfigKeys
    {
        public const string CafeName = "cafe_name";
        public const string PrintPriceBW = "print_price_bw";
        public const string PrintPriceColor = "print_price_color";
        public const string WarningMinutes1 = "warning_minutes_1";
        public const string WarningMinutes2 = "warning_minutes_2";
        public const string WarningMinutes3 = "warning_minutes_3";
        public const string LockScreenMessage = "lock_screen_message";
        public const string AutoBackupEnabled = "auto_backup_enabled";
        public const string BackupIntervalHours = "backup_interval_hours";
        public const string PointsPerDollar = "points_per_dollar";
        public const string InactivityLockMinutes = "inactivity_lock_minutes";
    }

    public static class ProductCategories
    {
        public const string Drinks = "Bebidas";
        public const string Snacks = "Snacks";
        public const string Stationery = "Papeleria";
        public const string Other = "Otros";
    }

    public static class GameCategories
    {
        public const string FPS = "FPS";
        public const string MOBA = "MOBA";
        public const string RPG = "RPG";
        public const string Sports = "Deportes";
        public const string Racing = "Carreras";
        public const string Strategy = "Estrategia";
        public const string Other = "Otros";
    }
}
