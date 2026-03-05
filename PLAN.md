# Desktopcafe - Plan de Desarrollo

## Software de Gestion de Cibercafe / Centro de Internet

**Stack:** C# .NET 9 + WinUI 3 (Windows App SDK) + Fluent Design
**Arquitectura:** Desktop con servidor local (SQLite + WAL mode)
**Alcance:** Una sola ubicacion, todos los modulos
**Rendimiento:** AOT compilation, async/await, pooling de conexiones, cache en memoria

---

## 1. Arquitectura General

```
┌───────────────────────────────────────────────────────────────┐
│                     PC SERVIDOR (Admin)                       │
│  ┌───────────────┐  ┌────────────┐  ┌─────────────────────┐  │
│  │  WinUI 3 App  │  │ SignalR    │  │  SQLite (WAL mode)  │  │
│  │  Fluent UI    │  │ Hub       │  │  + MemoryCache      │  │
│  │               │  │            │  │                     │  │
│  │ - Dashboard   │  │ WebSocket  │  │  - Sesiones         │  │
│  │ - Timer       │  │ :5000     │  │  - Clientes         │  │
│  │ - POS         │  │            │  │  - Productos        │  │
│  │ - Reportes    │  │ Bidirec-  │  │  - Ventas           │  │
│  │ - Inventario  │  │ cional    │  │  - Empleados        │  │
│  │ - Gaming      │  │            │  │  - Config           │  │
│  └───────┬───────┘  └─────┬──────┘  └─────────────────────┘  │
│          │                │                                    │
│          └────────────────┘                                    │
│                    │  Red Local (LAN)                          │
└────────────────────┼──────────────────────────────────────────┘
                     │  SignalR WebSocket
        ┌────────────┼────────────┐
        │            │            │
   ┌────▼────┐  ┌────▼────┐  ┌────▼────┐
   │  PC #1  │  │  PC #2  │  │  PC #N  │
   │ WinUI 3 │  │ WinUI 3 │  │ WinUI 3 │
   │ Agent   │  │ Agent   │  │ Agent   │
   │ Service │  │ Service │  │ Service │
   └─────────┘  └─────────┘  └─────────┘
```

### Componentes (5 proyectos):
1. **Desktopcafe.Server** - App WinUI 3 del servidor/admin (Fluent Design)
2. **Desktopcafe.Agent** - App WinUI 3 + Worker Service en PCs cliente
3. **Desktopcafe.Core** - Libreria compartida (modelos, DTOs, interfaces)
4. **Desktopcafe.Data** - Capa de datos (EF Core + SQLite + cache)
5. **Desktopcafe.Shared** - Contratos SignalR, constantes, validaciones

---

## 2. UI/UX - Filosofia de Diseno

### Principios de Fluent Design aplicados:
- **Mica / Acrylic backdrop** - Fondo translucido con efecto de profundidad
- **Rounded corners (8px)** - Esquinas redondeadas en todos los controles
- **Depth & shadows** - Sombras sutiles para jerarquia visual
- **Motion** - Animaciones fluidas de entrada/salida (Connected Animations)
- **Responsive layout** - Se adapta a diferentes tamanos de pantalla
- **Dark/Light theme** - Cambio automatico o manual, respetando tema de Windows
- **Typography** - Segoe UI Variable con escala tipografica consistente
- **Color system** - Paleta basada en accent color del sistema + semantica

### Paleta de colores:

```
Accent primario:     #0078D4 (Azul Fluent)
Accent secundario:   #005A9E
Exito / Disponible:  #0F7B0F (Verde)
Advertencia:         #9D5D00 (Ambar)
Error / Ocupado:     #C42B1C (Rojo)
Mantenimiento:       #767676 (Gris)

Backgrounds (Light):
  Layer 0: Mica (transparente, toma color del wallpaper)
  Layer 1: #FAFAFA
  Layer 2: #F3F3F3
  Layer 3: #EBEBEB

Backgrounds (Dark):
  Layer 0: Mica Dark
  Layer 1: #2D2D2D
  Layer 2: #333333
  Layer 3: #3D3D3D
```

### Layout principal:

```
┌─────────────────────────────────────────────────────────────────────────┐
│  [Mica Backdrop]                                                       │
│  ┌─ Title Bar (personalizado, arrastrable) ───────────────────────────┐│
│  │  ◉ Desktopcafe          🔍 Buscar (Ctrl+K)     👤 Admin  ⚙  🔔 ─ □ ×│
│  └────────────────────────────────────────────────────────────────────┘│
│  ┌──────────┬─────────────────────────────────────────────────────────┐│
│  │ Nav      │                                                         ││
│  │ [Fluent  │  ┌─ Breadcrumb ────────────────────────────────────┐   ││
│  │  NavView]│  │  Dashboard > Vista general                       │   ││
│  │          │  └─────────────────────────────────────────────────┘   ││
│  │ 🏠 Home  │                                                         ││
│  │          │  ┌─ KPI Cards (Acrylic) ───────────────────────────┐   ││
│  │ 🖥 PCs   │  │ ┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────┐│   ││
│  │          │  │ │💰 $1,250 │ │📊 12     │ │🖥 8/15  │ │🖨 23 ││   ││
│  │ ⏱ Timer │  │ │ Ingresos │ │ Sesiones │ │ PCs Uso  │ │Impres││   ││
│  │          │  │ │ ▲ +15%   │ │ Activas  │ │ 53%      │ │Hoy   ││   ││
│  │ 🛒 POS  │  │ └──────────┘ └──────────┘ └──────────┘ └──────┘│   ││
│  │          │  └─────────────────────────────────────────────────┘   ││
│  │ 👥 Users │                                                         ││
│  │          │  ┌─ Computer Grid ─────────────────────────────────┐   ││
│  │ 🖨 Print │  │                                                   │   ││
│  │          │  │  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐         │   ││
│  │ 📦 Stock │  │  │ PC01 │  │ PC02 │  │ PC03 │  │ PC04 │         │   ││
│  │          │  │  │ 🟢   │  │ ⚪   │  │ 🟢   │  │ ⚪   │         │   ││
│  │ 👷 Staff │  │  │ 0:45 │  │ FREE │  │ 1:20 │  │ FREE │         │   ││
│  │          │  │  │ Juan │  │      │  │ Maria│  │      │         │   ││
│  │ 📊 Stats │  │  └──────┘  └──────┘  └──────┘  └──────┘         │   ││
│  │          │  │  ┌──────┐  ┌──────┐  ┌──────┐  ┌──────┐         │   ││
│  │ 📶 WiFi  │  │  │ PC05 │  │ PC06 │  │ PC07 │  │ PC08 │         │   ││
│  │          │  │  │ 🔴   │  │ ⚪   │  │ 🟡   │  │ 🟢   │         │   ││
│  │ 🎮 Games │  │  │ 0:02 │  │ FREE │  │ MANT │  │ 2:00 │         │   ││
│  │          │  │  │ Pedro│  │      │  │      │  │ Luis │         │   ││
│  │ 📅 Rsrvs │  │  └──────┘  └──────┘  └──────┘  └──────┘         │   ││
│  │          │  └─────────────────────────────────────────────────┘   ││
│  │ ─────── │                                                         ││
│  │ ⚙ Config│  ┌─ Activity Feed ─────┐  ┌─ Alerts ──────────────┐   ││
│  │          │  │ 10:23 Coca-Cola $15 │  │ ⚠ PC05: 2 min !!     │   ││
│  │          │  │ 10:20 Sesion PC03   │  │ ⚠ Stock bajo: Agua   │   ││
│  │          │  │ 10:15 Impresion 3pg │  │ ℹ PC02 disponible    │   ││
│  │          │  └─────────────────────┘  └───────────────────────┘   ││
│  └──────────┴─────────────────────────────────────────────────────────┘│
│  ┌─ Status Bar (InfoBar) ────────────────────────────────────────────┐│
│  │  👷 Juan Perez  │  💰 Caja: $3,450  │  🖥 15 PCs  │  🕐 08:00-16:00 ││
│  └───────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────────┘
```

### Componentes WinUI 3 utilizados:
- `NavigationView` - Navegacion lateral colapsable con iconos Fluent
- `TabView` - Pestanas para multiples ventanas abiertas
- `InfoBar` - Notificaciones contextuales (exito, error, advertencia)
- `TeachingTip` - Tooltips interactivos para onboarding
- `ContentDialog` - Dialogos modales con blur de fondo
- `CommandBar` - Barra de acciones contextual
- `BreadcrumbBar` - Navegacion jerárquica
- `ProgressRing` - Indicadores de carga animados
- `NumberBox` - Entrada numerica con validacion
- `CalendarDatePicker` - Selector de fecha nativo
- `TreeView` - Vista de arbol para categorias
- `DataGrid` - Tablas con ordenamiento, filtrado, paginacion (CommunityToolkit)
- `Expander` - Secciones colapsables
- `PersonPicture` - Avatares de clientes/empleados

### Tarjeta de Computadora (ComputerCard) - Diseno:

```
┌─────────────────────────┐     ┌─────────────────────────┐
│  [Acrylic + Green tint] │     │  [Acrylic + Neutral]    │
│                         │     │                         │
│    🖥  PC-01            │     │    🖥  PC-02            │
│    ━━━━━━━━━━━━━━━━━━   │     │                         │
│                         │     │                         │
│    ⏱  01:23:45          │     │    Disponible            │
│    ████████████░░░ 78%  │     │                         │
│                         │     │    Haz clic para         │
│    👤 Juan Garcia       │     │    iniciar sesion        │
│    💰 $35.00            │     │                         │
│                         │     │                         │
│  [Extender] [Pausar] [⋯]│    │      [Iniciar Sesion]    │
└─────────────────────────┘     └─────────────────────────┘

Estado: En uso                   Estado: Libre

┌─────────────────────────┐     ┌─────────────────────────┐
│  [Acrylic + Red tint]   │     │  [Acrylic + Gray tint]  │
│                         │     │                         │
│    🖥  PC-05            │     │    🖥  PC-07            │
│    ━━━━━━━━━━━━━━━━━━   │     │    ━━━━━━━━━━━━━━━━━━   │
│                         │     │                         │
│    ⏱  00:01:30          │     │    🔧 En Mantenimiento  │
│    █████████████████ 98%│     │                         │
│    ⚠ TIEMPO POR ACABAR │     │    Mouse danado          │
│                         │     │    Reportado: 10:30am   │
│    👤 Pedro Lopez       │     │                         │
│    💰 $20.00            │     │                         │
│  [Extender] [Terminar]  │     │    [Marcar Listo]       │
└─────────────────────────┘     └─────────────────────────┘

Estado: Por expirar               Estado: Mantenimiento
```

### Dialogo de Nueva Sesion:

```
┌─────────────────────────────────────────┐
│  ╳                                      │
│                                          │
│     Nueva Sesion - PC01                  │
│     ━━━━━━━━━━━━━━━━━━━━━━              │
│                                          │
│  👤 Cliente (opcional)                   │
│  ┌─────────────────────────────────┐     │
│  │ 🔍 Buscar cliente...            │     │
│  └─────────────────────────────────┘     │
│                                          │
│  Tipo de sesion:                         │
│  ┌───────────┐ ┌───────────┐ ┌────────┐ │
│  │ ⏱ Prepago │ │ 📋 Postpago│ │ ∞ Libre│ │
│  │  (activo) │ │           │ │        │ │
│  └───────────┘ └───────────┘ └────────┘ │
│                                          │
│  Duracion:                               │
│  ┌────┐  ┌────┐  ┌────┐  ┌──────────┐  │
│  │15m │  │30m │  │ 1h │  │Personaliz│  │
│  └────┘  └────┘  └────┘  └──────────┘  │
│                                          │
│  Tarifa: $20.00/hora                     │
│  Total:  $20.00                          │
│                                          │
│  ┌──────────────────────────────────┐    │
│  │        ▶  Iniciar Sesion         │    │
│  └──────────────────────────────────┘    │
│                                          │
└──────────────────────────────────────────┘
```

### Pantalla de Bloqueo del Agente (PC Cliente):

```
┌─────────────────────────────────────────────────────────────┐
│ [Mica Dark + Acrylic Blur - Pantalla completa]              │
│                                                              │
│                                                              │
│                                                              │
│                    ◉ Desktopcafe                             │
│                                                              │
│                    🖥  PC-01                                 │
│                                                              │
│              ┌──────────────────────┐                        │
│              │                      │                        │
│              │   Esta computadora   │                        │
│              │   esta bloqueada     │                        │
│              │                      │                        │
│              │   Solicite acceso    │                        │
│              │   en el mostrador    │                        │
│              │                      │                        │
│              └──────────────────────┘                        │
│                                                              │
│              ┌──────────────────────┐                        │
│              │  📶 WiFi disponible  │                        │
│              │  Pregunte por un     │                        │
│              │  voucher de acceso   │                        │
│              └──────────────────────┘                        │
│                                                              │
│                                        10:30 AM  05/03/2026 │
└─────────────────────────────────────────────────────────────┘
```

### Pantalla activa del Agente (sesion en curso):

```
┌─────────────────────────────────────────┐
│  [Floating overlay - esquina superior]   │
│                                          │
│   ⏱ 01:23:45    💰 $20.00   [_] [▼]    │
│                                          │
└──────────────────────────────────────────┘

Expandido:
┌──────────────────────────────────────────┐
│  Desktopcafe - PC01                      │
│  ━━━━━━━━━━━━━━━━━━━━                   │
│                                           │
│  ⏱  Tiempo restante:  01:23:45           │
│  ██████████████░░░░░░  70%               │
│                                           │
│  👤  Juan Garcia                          │
│  💰  Total: $20.00                        │
│  🖨  Impresiones: 3 paginas ($6.00)      │
│                                           │
│  [Solicitar mas tiempo]                   │
│  [Llamar al admin]                        │
└──────────────────────────────────────────┘
```

---

## 3. Estructura del Proyecto

```
Desktopcafe/
├── src/
│   ├── Desktopcafe.sln
│   │
│   ├── Desktopcafe.Core/                  # Libreria compartida (.NET 9)
│   │   ├── Models/
│   │   │   ├── Client.cs
│   │   │   ├── Computer.cs
│   │   │   ├── Session.cs
│   │   │   ├── Product.cs
│   │   │   ├── Sale.cs
│   │   │   ├── SaleItem.cs
│   │   │   ├── Employee.cs
│   │   │   ├── PrintJob.cs
│   │   │   ├── Reservation.cs
│   │   │   ├── Shift.cs
│   │   │   ├── WiFiVoucher.cs
│   │   │   ├── Game.cs
│   │   │   └── AuditLog.cs
│   │   ├── DTOs/
│   │   │   ├── SessionDto.cs
│   │   │   ├── SaleDto.cs
│   │   │   ├── ComputerStatusDto.cs
│   │   │   ├── DashboardDto.cs
│   │   │   └── ReportDto.cs
│   │   ├── Enums/
│   │   │   ├── SessionType.cs            # Prepago, Postpago, Libre
│   │   │   ├── ComputerStatus.cs         # Disponible, Ocupada, Mantenimiento
│   │   │   ├── PaymentMethod.cs          # Efectivo, Tarjeta, Transferencia
│   │   │   └── UserRole.cs              # Admin, Cajero
│   │   └── Interfaces/
│   │       ├── ISessionService.cs
│   │       ├── IBillingService.cs
│   │       ├── IComputerService.cs
│   │       └── IReportService.cs
│   │
│   ├── Desktopcafe.Shared/               # Contratos compartidos
│   │   ├── Hubs/
│   │   │   ├── IServerHub.cs             # Metodos del servidor
│   │   │   └── IAgentHub.cs              # Metodos del agente
│   │   ├── Constants.cs
│   │   └── Validators/
│   │       ├── SessionValidator.cs
│   │       └── SaleValidator.cs
│   │
│   ├── Desktopcafe.Data/                  # Capa de datos
│   │   ├── AppDbContext.cs               # EF Core DbContext
│   │   ├── DesignTimeFactory.cs          # Para migraciones
│   │   ├── Migrations/
│   │   ├── Configuration/
│   │   │   ├── ComputerConfiguration.cs  # Fluent API config
│   │   │   ├── SessionConfiguration.cs
│   │   │   └── SaleConfiguration.cs
│   │   ├── Repositories/
│   │   │   ├── Repository.cs             # Generic base
│   │   │   ├── SessionRepository.cs
│   │   │   ├── SaleRepository.cs
│   │   │   └── ReportRepository.cs
│   │   └── Seeding/
│   │       └── InitialDataSeeder.cs      # Datos iniciales
│   │
│   ├── Desktopcafe.Server/                # App WinUI 3 (Servidor)
│   │   ├── App.xaml / App.xaml.cs
│   │   ├── MainWindow.xaml               # Shell con NavigationView
│   │   ├── Activation/
│   │   │   └── AppActivationService.cs   # Startup, single instance
│   │   ├── Views/
│   │   │   ├── DashboardPage.xaml        # Pagina principal
│   │   │   ├── ComputerMapPage.xaml      # Mapa de PCs (GridView)
│   │   │   ├── TimerPage.xaml            # Control de sesiones
│   │   │   ├── POSPage.xaml              # Punto de venta
│   │   │   ├── ClientsPage.xaml          # Gestion de clientes
│   │   │   ├── PrinterPage.xaml          # Impresoras
│   │   │   ├── InventoryPage.xaml        # Inventario
│   │   │   ├── ReportsPage.xaml          # Reportes + graficas
│   │   │   ├── EmployeesPage.xaml        # Empleados + turnos
│   │   │   ├── ReservationsPage.xaml     # Reservaciones
│   │   │   ├── WiFiPage.xaml             # Vouchers WiFi
│   │   │   ├── GamingPage.xaml           # Centro gaming
│   │   │   ├── SettingsPage.xaml         # Configuracion
│   │   │   └── LoginPage.xaml            # Login de empleado
│   │   ├── ViewModels/
│   │   │   ├── DashboardViewModel.cs
│   │   │   ├── ComputerMapViewModel.cs
│   │   │   ├── TimerViewModel.cs
│   │   │   ├── POSViewModel.cs
│   │   │   ├── ClientsViewModel.cs
│   │   │   ├── PrinterViewModel.cs
│   │   │   ├── InventoryViewModel.cs
│   │   │   ├── ReportsViewModel.cs
│   │   │   ├── EmployeesViewModel.cs
│   │   │   ├── ReservationsViewModel.cs
│   │   │   ├── WiFiViewModel.cs
│   │   │   ├── GamingViewModel.cs
│   │   │   └── SettingsViewModel.cs
│   │   ├── Services/
│   │   │   ├── NavigationService.cs
│   │   │   ├── ThemeService.cs           # Light/Dark/System
│   │   │   ├── NotificationService.cs    # InfoBar + Toast
│   │   │   ├── DialogService.cs          # ContentDialog manager
│   │   │   ├── SignalRHostService.cs     # Hosted SignalR server
│   │   │   ├── SessionTimerService.cs    # Timer engine
│   │   │   ├── BillingService.cs
│   │   │   ├── PrinterMonitorService.cs
│   │   │   ├── WiFiService.cs
│   │   │   ├── BackupService.cs          # Auto-backup SQLite
│   │   │   └── SoundService.cs           # Sonidos de alerta
│   │   ├── Controls/
│   │   │   ├── ComputerCard.xaml         # Tarjeta con estados
│   │   │   ├── TimerRing.xaml            # Anillo de progreso
│   │   │   ├── KpiCard.xaml              # Tarjeta de metrica
│   │   │   ├── ActivityFeed.xaml         # Feed de actividad
│   │   │   ├── QuickSalePanel.xaml       # Panel de venta rapida
│   │   │   └── AlertBadge.xaml           # Badge de notificacion
│   │   ├── Dialogs/
│   │   │   ├── NewSessionDialog.xaml     # Iniciar sesion
│   │   │   ├── ExtendSessionDialog.xaml  # Extender tiempo
│   │   │   ├── CheckoutDialog.xaml       # Cobro final
│   │   │   ├── NewClientDialog.xaml      # Registrar cliente
│   │   │   ├── CashRegisterDialog.xaml   # Apertura/cierre caja
│   │   │   └── ConfirmDialog.xaml        # Confirmacion generica
│   │   ├── Converters/
│   │   │   ├── StatusToColorConverter.cs
│   │   │   ├── TimeSpanToStringConverter.cs
│   │   │   ├── CurrencyConverter.cs
│   │   │   └── BoolToVisibilityConverter.cs
│   │   ├── Helpers/
│   │   │   ├── WindowHelper.cs           # Mica backdrop setup
│   │   │   └── TitleBarHelper.cs         # Custom title bar
│   │   ├── Themes/
│   │   │   ├── AppColors.xaml            # Paleta de colores
│   │   │   ├── AppStyles.xaml            # Estilos globales
│   │   │   └── ComputerCardStyles.xaml   # Estilos por estado
│   │   └── Assets/
│   │       ├── Fonts/
│   │       │   └── SegoeFluentIcons.ttf
│   │       ├── Images/
│   │       │   ├── logo.png
│   │       │   └── splash.png
│   │       └── Sounds/
│   │           ├── alert.wav
│   │           ├── session-start.wav
│   │           └── session-end.wav
│   │
│   └── Desktopcafe.Agent/                # App en PCs cliente
│       ├── App.xaml / App.xaml.cs
│       ├── Program.cs                    # Single instance + autostart
│       ├── Services/
│       │   ├── AgentWorkerService.cs     # Background worker
│       │   ├── SignalRClientService.cs   # Conexion al servidor
│       │   ├── ScreenLockService.cs      # Bloqueo/desbloqueo
│       │   ├── ProcessMonitorService.cs  # Monitor de procesos
│       │   ├── PrintInterceptService.cs  # Interceptor impresion
│       │   ├── HardwareInfoService.cs    # CPU, RAM, disco
│       │   └── GameLauncherService.cs    # Lanzador de juegos
│       ├── Views/
│       │   ├── LockScreenPage.xaml       # Pantalla de bloqueo
│       │   ├── SessionOverlay.xaml       # Overlay flotante
│       │   └── GameBrowserPage.xaml      # Explorador de juegos
│       └── ViewModels/
│           ├── LockScreenViewModel.cs
│           └── SessionOverlayViewModel.cs
│
├── tests/
│   ├── Desktopcafe.Core.Tests/
│   ├── Desktopcafe.Data.Tests/
│   └── Desktopcafe.Server.Tests/
│
├── PLAN.md
├── .gitignore
├── .editorconfig
└── LICENSE
```

---

## 4. Modulos Detallados

### 4.1 Timer / Control de Tiempo (Modulo Principal)
**Prioridad: CRITICA**

Funcionalidades:
- Iniciar sesion: Prepago (paga antes), Postpago (paga al salir), Libre (sin limite)
- Temporizador en tiempo real con countdown visible
- **TimerRing** visual animado (anillo circular de progreso)
- Alertas automaticas: 5 min, 2 min, 1 min antes de terminar
- Sonido de alerta configurable
- Extension de tiempo durante sesion activa (sin interrumpir)
- Pausa de sesion (para ir al bano, etc.)
- Bloqueo automatico de PC al terminar tiempo
- Tarifas configurables: por hora, media hora, 15 min, personalizadas
- Tarifas diferenciadas: horario normal, nocturno, fin de semana
- Descuentos para clientes frecuentes

Flujo:
```
Cliente llega → Seleccionar PC (click en tarjeta) → ContentDialog de sesion →
Elegir tipo y duracion → Iniciar (Connected Animation) → Timer activo →
InfoBar de alerta pre-expiracion → Bloqueo al terminar → CheckoutDialog
```

### 4.2 Control de Impresoras
**Prioridad: ALTA**

Funcionalidades:
- Deteccion automatica de impresoras en red
- Cobro por pagina impresa (B/N y color con precios diferentes)
- Contador de paginas por sesion
- Cola de impresion visible en panel admin con ListView
- Aprobar/rechazar trabajos de impresion con SwipeControl
- Historial de impresiones por cliente
- Limite de paginas por sesion (configurable)
- Soporte para impresora de tickets (recibos POS)

### 4.3 Punto de Venta (POS)
**Prioridad: ALTA**

Funcionalidades:
- Catalogo de productos con categorias (GridView con imagenes)
- Busqueda rapida con AutoSuggestBox
- Carrito de compras con drag & drop
- Metodos de pago: efectivo, tarjeta, transferencia
- Calculo de cambio automatico con NumberBox
- Vincular ventas a sesion activa (cobro consolidado)
- Ticket/recibo impreso o digital
- Apertura/cierre de caja con arqueo
- Historial de ventas del dia con DataGrid

### 4.4 Control de WiFi
**Prioridad: MEDIA**

Funcionalidades:
- Generacion de vouchers WiFi con tiempo limitado
- Codigos QR para acceso rapido (QRCoder)
- Ancho de banda configurable por voucher
- Monitoreo de dispositivos conectados
- Bloqueo de dispositivos
- Integracion con router (MikroTik API o similar)

### 4.5 Gaming Center
**Prioridad: MEDIA**

Funcionalidades:
- Lanzador de juegos instalados en cada PC (GridView con covers)
- Categorias de juegos (FPS, MOBA, RPG, etc.)
- Registro de juegos mas jugados
- Perfil de jugador con estadisticas (PersonPicture + datos)
- Sistema basico de torneos (bracket simple)
- Tarifas especiales para gaming

### 4.6 Gestion de Clientes
**Prioridad: ALTA**

Funcionalidades:
- Registro rapido (nombre, telefono, correo)
- Tarjeta de cliente / codigo unico
- Historial de sesiones y compras (TabView con listas)
- Sistema de puntos/lealtad
- Saldo prepago (recargas)
- Clientes frecuentes con descuento automatico
- Avatar con PersonPicture

### 4.7 Gestion de Empleados
**Prioridad: MEDIA**

Funcionalidades:
- Registro de empleados con roles (admin, cajero)
- Control de turnos (entrada/salida con hora)
- Permisos por rol (quien puede dar descuentos, reembolsos, etc.)
- Registro de acciones (auditoria en AuditLog)
- Reporte de ventas por empleado

### 4.8 Inventario
**Prioridad: MEDIA**

Funcionalidades:
- Catalogo de productos con stock
- Alertas de stock bajo (InfoBar persistente)
- Registro de entradas (compras a proveedor)
- Registro de salidas (ventas automaticas)
- Historial de movimientos
- Reporte de productos mas vendidos

### 4.9 Reservaciones
**Prioridad: BAJA**

Funcionalidades:
- Reservar PC para horario especifico (CalendarDatePicker + TimePicker)
- Reservar para grupos/eventos
- Notificacion de reserva proxima
- Cancelacion con politica configurable

### 4.10 Reportes y Estadisticas
**Prioridad: ALTA**

Funcionalidades:
- Dashboard en tiempo real con KpiCards animadas
- Graficas interactivas con LiveCharts2:
  - Ingresos diarios (linea)
  - Uso por hora (barras)
  - Distribucion de ventas (pie)
  - Ocupacion de PCs (area)
- Reportes generables:
  - Ingresos diarios/semanales/mensuales
  - Horas pico de uso
  - Productos mas vendidos
  - Clientes mas frecuentes
  - Uso por PC (identificar PCs con problemas)
  - Impresiones totales y costo
  - Rendimiento por empleado
- Exportar a Excel (ClosedXML) y PDF (QuestPDF)
- Rango de fechas personalizable con CalendarDatePicker

---

## 5. Modelo de Base de Datos

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Computers   │     │   Sessions   │     │   Clients    │
├──────────────┤     ├──────────────┤     ├──────────────┤
│ Id (int PK)  │◄────│ ComputerId   │     │ Id (int PK)  │
│ Name         │     │ ClientId     │────►│ Name         │
│ IpAddress    │     │ EmployeeId   │     │ Phone        │
│ MacAddress   │     │ StartTime    │     │ Email        │
│ Status (enum)│     │ EndTime      │     │ Code (unique)│
│ Zone         │     │ PlannedEnd   │     │ Balance      │
│ IsGaming     │     │ SessionType  │     │ Points       │
│ Specs (json) │     │ RatePerHour  │     │ AvatarPath   │
│ Position X/Y │     │ TotalCharge  │     │ IsActive     │
│ LastHeartbeat│     │ IsPaused     │     │ CreatedAt    │
│ IsOnline     │     │ PausedAt     │     └──────────────┘
└──────────────┘     │ Status       │
                     │ Notes        │     ┌──────────────┐
                     │ CreatedAt    │     │  Employees   │
                     └──────────────┘     ├──────────────┤
                                          │ Id (int PK)  │
┌──────────────┐     ┌──────────────┐     │ Name         │
│  Products    │     │    Sales     │     │ Username     │
├──────────────┤     ├──────────────┤     │ PasswordHash │
│ Id (int PK)  │     │ Id (int PK)  │     │ Role (enum)  │
│ Name         │     │ EmployeeId   │────►│ IsActive     │
│ Category     │     │ ClientId     │     │ CreatedAt    │
│ Price        │     │ SessionId    │     └──────────────┘
│ Cost         │     │ Total        │
│ Stock        │     │ PaymentMethod│     ┌──────────────┐
│ MinStock     │     │ CreatedAt    │     │  PrintJobs   │
│ ImagePath    │     └──────────────┘     ├──────────────┤
│ Barcode      │           │              │ Id (int PK)  │
│ IsActive     │     ┌─────▼──────┐       │ SessionId    │
└──────────────┘     │ SaleItems  │       │ ComputerId   │
      │              ├────────────┤       │ Pages        │
      │              │ Id (int PK)│       │ IsColor      │
      └─────────────►│ SaleId     │       │ Cost         │
                     │ ProductId  │       │ Status       │
                     │ Quantity   │       │ DocumentName │
                     │ UnitPrice  │       │ PrinterName  │
                     │ Subtotal   │       │ CreatedAt    │
                     └────────────┘       └──────────────┘

┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│ Reservations │     │   Shifts     │     │  AuditLogs   │
├──────────────┤     ├──────────────┤     ├──────────────┤
│ Id (int PK)  │     │ Id (int PK)  │     │ Id (int PK)  │
│ ComputerId   │     │ EmployeeId   │     │ EmployeeId   │
│ ClientId     │     │ StartTime    │     │ Action       │
│ StartTime    │     │ EndTime      │     │ EntityType   │
│ Duration     │     │ CashStart    │     │ EntityId     │
│ Status       │     │ CashEnd      │     │ Details(json)│
│ Notes        │     │ Notes        │     │ CreatedAt    │
│ CreatedAt    │     └──────────────┘     └──────────────┘
└──────────────┘

┌──────────────┐     ┌──────────────┐
│ WiFiVouchers │     │    Games     │
├──────────────┤     ├──────────────┤
│ Id (int PK)  │     │ Id (int PK)  │
│ Code (unique)│     │ Name         │
│ DurationMins │     │ Category     │
│ IsUsed       │     │ ExePath      │
│ MacAddress   │     │ CoverPath    │
│ CreatedAt    │     │ TimesPlayed  │
│ UsedAt       │     │ IsActive     │
│ ExpiresAt    │     └──────────────┘
└──────────────┘

┌──────────────┐
│  RateConfigs │
├──────────────┤
│ Id (int PK)  │
│ Name         │
│ PricePerHour │
│ PricePerHalf │
│ PricePer15   │
│ IsWeekend    │
│ IsNightRate  │
│ StartHour    │
│ EndHour      │
│ IsDefault    │
└──────────────┘
```

---

## 6. Tecnologias y Librerias

| Componente | Tecnologia | Por que |
|---|---|---|
| Runtime | **.NET 9** | Ultimo LTS, mejor rendimiento, AOT |
| UI Framework | **WinUI 3 (Windows App SDK 1.6+)** | Fluent Design nativo, moderno |
| UI Toolkit | **WinUI Community Toolkit** | DataGrid, controles extra |
| Patron UI | **MVVM con CommunityToolkit.Mvvm** | Source generators, alto rendimiento |
| Base de Datos | **SQLite + EF Core 9** | Sin servidor, WAL mode, rapido |
| Cache | **Microsoft.Extensions.Caching.Memory** | Cache en RAM para consultas frecuentes |
| Comunicacion | **SignalR (ASP.NET Core)** | WebSocket bidireccional, reconexion auto |
| Graficas | **LiveCharts2** | Animaciones fluidas, WinUI 3 compatible |
| Reportes PDF | **QuestPDF** | API fluida, rapido, moderno |
| Reportes Excel | **ClosedXML** | Crear Excel sin Office instalado |
| QR Codes | **QRCoder** | Generacion de QR para WiFi vouchers |
| Impresoras | **System.Drawing + Win32 Spooler API** | Control total de impresion |
| Hashing | **BCrypt.Net-Next** | Hashing seguro de contrasenas |
| Validacion | **FluentValidation** | Validacion declarativa |
| Logging | **Serilog + Serilog.Sinks.File** | Logging estructurado a archivo |
| DI | **Microsoft.Extensions.DependencyInjection** | DI nativo de .NET |
| JSON | **System.Text.Json** | Mas rapido que Newtonsoft, nativo |
| Mapeo | **Mapster** | Mapeo DTO mas rapido que AutoMapper |
| Iconos | **Segoe Fluent Icons** | Iconos nativos de Windows 11 |
| Instalador | **Inno Setup** | Instalador profesional, gratuito |
| Tests | **xUnit + NSubstitute + FluentAssertions** | Stack de testing moderno |
| CI local | **Nuke Build** | Build automation en C# |

---

## 7. Optimizaciones de Rendimiento

### 7.1 Base de Datos
```csharp
// SQLite en modo WAL (Write-Ahead Logging) - lecturas no bloquean escrituras
optionsBuilder.UseSqlite("Data Source=desktopcafe.db", opt =>
{
    opt.CommandTimeout(30);
});
// Pragma WAL mode en OnConfiguring
connection.Execute("PRAGMA journal_mode=WAL;");
connection.Execute("PRAGMA synchronous=NORMAL;");
connection.Execute("PRAGMA cache_size=-20000;"); // 20MB cache
```

### 7.2 Cache en Memoria
```csharp
// Cache de datos frecuentes (tarifas, productos, config)
services.AddMemoryCache();
// Cache de dashboard KPIs con expiracion de 10 segundos
var kpis = await _cache.GetOrCreateAsync("dashboard_kpis", async entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
    return await _reportRepo.GetDashboardKpisAsync();
});
```

### 7.3 UI
- **Virtualizacion** en todas las listas largas (ItemsRepeater con virtualizacion)
- **x:Load** para carga diferida de controles no visibles
- **Compiled Bindings (x:Bind)** en vez de Binding clasico (10x mas rapido)
- **Incremental loading** para DataGrid con muchos registros
- **Deferred rendering** para paginas pesadas (reportes)
- **Connected Animations** para transiciones fluidas entre vistas

### 7.4 Comunicacion
- **SignalR con MessagePack** para serialización binaria (mas rapido que JSON)
- **Reconexion automatica** con backoff exponencial
- **Heartbeat cada 30 seg** en vez de polling constante
- **Batch updates** para actualizar multiples PCs en una sola operacion

### 7.5 General
- **Async/await** en todas las operaciones I/O (cero bloqueo de UI thread)
- **CancellationToken** en operaciones largas
- **IAsyncEnumerable** para streaming de datos grandes
- **Object pooling** para objetos frecuentes (DTOs)
- **Compiled queries** en EF Core para consultas frecuentes
- **ReadyToRun (R2R) compilation** para startup rapido

---

## 8. Comunicacion Servidor-Agente

Protocolo: **SignalR** sobre la red local (LAN) con **MessagePack**

### Servidor → Agente (IAgentHub):
```csharp
public interface IAgentHub
{
    Task LockScreen();                          // Bloquear pantalla
    Task UnlockScreen(SessionDto session);      // Desbloquear con datos de sesion
    Task UpdateTimer(TimeSpan remaining);       // Actualizar countdown
    Task ShowWarning(string message, int mins); // Alerta de tiempo
    Task ShowMessage(string title, string msg); // Mensaje general
    Task Shutdown();                            // Apagar PC
    Task Restart();                             // Reiniciar PC
    Task RequestScreenshot();                   // Captura de pantalla
    Task UpdateGameCatalog(List<GameDto> games);// Actualizar juegos
    Task UpdateConfig(AgentConfigDto config);   // Config del agente
}
```

### Agente → Servidor (IServerHub):
```csharp
public interface IServerHub
{
    Task Register(ComputerRegistrationDto info);     // Registrar PC
    Task Heartbeat(HeartbeatDto status);              // Status periodico
    Task PrintJobDetected(PrintJobDto job);           // Nueva impresion
    Task SessionRequest(SessionRequestDto request);   // Solicitud de sesion
    Task AlertAdmin(AlertDto alert);                  // Alerta
    Task SendScreenshot(byte[] imageData);            // Captura
    Task GameLaunched(int gameId);                    // Juego iniciado
}
```

---

## 9. Fases de Desarrollo

### FASE 1: Fundamentos + UI Shell (Semanas 1-2)
- [ ] Crear solucion .NET 9 con los 5 proyectos
- [ ] Configurar WinUI 3 con Windows App SDK
- [ ] Implementar Mica backdrop + custom title bar
- [ ] Crear NavigationView con todas las paginas (shell)
- [ ] Implementar tema Light/Dark con ThemeService
- [ ] Definir paleta de colores y estilos globales (AppColors.xaml)
- [ ] Configurar EF Core + SQLite (WAL mode) + migraciones
- [ ] Implementar modelos de datos base
- [ ] Sistema de autenticacion (LoginPage con ContentDialog)
- [ ] Configurar DI, Serilog, MemoryCache

### FASE 2: Timer, Mapa de PCs y Agente (Semanas 3-5)
- [ ] Disenar ComputerCard control con estados visuales
- [ ] Crear ComputerMapPage con GridView de tarjetas
- [ ] Implementar NewSessionDialog (ContentDialog)
- [ ] Desarrollar TimerRing control (anillo de progreso animado)
- [ ] Motor de temporizador (SessionTimerService)
- [ ] Desarrollar Desktopcafe.Agent (WinUI 3 + Worker Service)
- [ ] Pantalla de bloqueo del agente (LockScreenPage)
- [ ] Overlay flotante de sesion activa (SessionOverlay)
- [ ] Comunicacion SignalR servidor-agente con MessagePack
- [ ] Bloqueo/desbloqueo automatico
- [ ] Tarifas configurables (RateConfigs)
- [ ] Alertas de tiempo con sonido e InfoBar

### FASE 3: POS e Impresoras (Semanas 6-7)
- [ ] CRUD de productos con categorias (GridView con imagenes)
- [ ] POSPage con catalogo visual y carrito
- [ ] QuickSalePanel para ventas rapidas
- [ ] Metodos de pago con CheckoutDialog
- [ ] Apertura/cierre de caja (CashRegisterDialog)
- [ ] Ticket impreso con QuestPDF
- [ ] Deteccion de impresoras de red
- [ ] Interceptor de impresion en el agente
- [ ] Aprobacion de impresiones con SwipeControl
- [ ] Cobro por pagina (B/N vs color)

### FASE 4: Clientes y Empleados (Semanas 8-9)
- [ ] ClientsPage con DataGrid + busqueda (AutoSuggestBox)
- [ ] NewClientDialog con PersonPicture
- [ ] Sistema de saldo prepago y recargas
- [ ] Puntos de lealtad con reglas configurables
- [ ] Historial por cliente (TabView: sesiones, compras, impresiones)
- [ ] EmployeesPage con CRUD y roles
- [ ] Control de turnos (Shifts)
- [ ] Log de auditoria (AuditLogs)
- [ ] Permisos por rol

### FASE 5: Inventario, WiFi y Gaming (Semanas 10-11)
- [ ] InventoryPage con alertas de stock bajo
- [ ] Movimientos de inventario (entradas/salidas)
- [ ] WiFiPage con generacion de vouchers
- [ ] QR codes con QRCoder para vouchers
- [ ] Integracion basica con MikroTik (opcional)
- [ ] GamingPage con catalogo visual de juegos
- [ ] GameLauncherService en el agente
- [ ] GameBrowserPage en el agente
- [ ] ReservationsPage con CalendarDatePicker

### FASE 6: Reportes, Dashboard y Pulido (Semanas 12-13)
- [ ] DashboardPage con KpiCards animadas
- [ ] ActivityFeed en tiempo real
- [ ] Alertas con AlertBadge en NavigationView
- [ ] ReportsPage con graficas LiveCharts2
- [ ] Filtros de fecha y exportacion (PDF + Excel)
- [ ] Connected Animations entre paginas
- [ ] Refinar todos los estilos y animaciones
- [ ] Manejo global de errores con InfoBar
- [ ] Sonidos de alerta configurables

### FASE 7: Testing, Instalador y Despliegue (Semana 14)
- [ ] Tests unitarios (xUnit + NSubstitute)
- [ ] Tests de integracion con SQLite in-memory
- [ ] Wizard de primer uso (configuracion inicial)
- [ ] Crear instalador servidor (Inno Setup)
- [ ] Crear instalador agente (Inno Setup, autostart)
- [ ] ReadyToRun compilation para startup rapido
- [ ] Documentacion de usuario basica

---

## 10. Seguridad

- Contrasenas hasheadas con BCrypt (factor 12)
- Roles y permisos por empleado (Admin, Cajero)
- Bloqueo de sesion de admin por inactividad (5 min configurable)
- El agente corre como servicio Windows (no puede ser cerrado por el usuario)
- Proteccion contra desinstalacion del agente (requiere contrasena admin)
- Log de todas las operaciones financieras (AuditLog)
- Backup automatico de la base de datos (diario, 7 dias de retencion)
- SignalR sobre LAN solamente (sin exposicion a internet)
- Validacion con FluentValidation en todas las entradas
- Inyeccion de dependencias para testabilidad

---

## 11. Requisitos del Sistema

### Servidor (PC Admin):
- Windows 10 version 1809+ / Windows 11
- .NET 9 Runtime
- Windows App SDK Runtime
- 4 GB RAM minimo (8 GB recomendado)
- Red LAN configurada
- 500 MB espacio en disco

### Clientes (PCs del cafe):
- Windows 10 version 1809+ / Windows 11
- .NET 9 Runtime
- Windows App SDK Runtime
- 2 GB RAM minimo
- Conexion a la red LAN del servidor

---

## 12. Atajos de Teclado (Admin)

| Atajo | Accion |
|---|---|
| `Ctrl+K` | Busqueda rapida global |
| `Ctrl+N` | Nueva sesion |
| `Ctrl+S` | Nueva venta rapida |
| `Ctrl+1-9` | Navegar a seccion N |
| `F5` | Refrescar vista actual |
| `F11` | Pantalla completa |
| `Esc` | Cerrar dialogo/panel |
