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

### Benchmark Targets

| Metrica | Target | Medicion |
|---|---|---|
| **Startup del servidor** | < 2 segundos | Desde doble-clic hasta UI interactiva |
| **Startup del agente** | < 1 segundo | Desde boot hasta conectado al servidor |
| **Inicio de sesion (timer)** | < 100ms | Click en "Iniciar" hasta PC desbloqueada |
| **Bloqueo de PC** | < 200ms | Timer llega a 0 hasta pantalla bloqueada |
| **Renderizar mapa de PCs** | < 50ms | 50 PCs con actualizacion en tiempo real |
| **Busqueda de productos (POS)** | < 30ms | Filtrado de 500+ productos |
| **Generar reporte mensual** | < 3 segundos | 30 dias, todas las metricas + graficas |
| **Exportar PDF** | < 2 segundos | Reporte completo con graficas |
| **Uso de RAM servidor** | < 150 MB | Con 50 PCs activas |
| **Uso de RAM agente** | < 40 MB | En sesion activa |
| **Tamano BD (1 ano)** | < 500 MB | Uso intensivo, 30 PCs, POS activo |
| **Reconexion agente** | < 3 segundos | Despues de caida de red momentanea |

---

### 7.1 Base de Datos - SQLite Tuning Avanzado

```csharp
// === AppDbContext.cs - Configuracion optimizada de SQLite ===

protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    var conn = new SqliteConnection("Data Source=desktopcafe.db;Cache=Shared");
    conn.Open();

    // PRAGMAs de rendimiento (ejecutar una vez al abrir conexion)
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        PRAGMA journal_mode = WAL;          -- Write-Ahead Logging: lecturas no bloquean escrituras
        PRAGMA synchronous = NORMAL;        -- Balance entre seguridad y velocidad
        PRAGMA cache_size = -20000;         -- 20MB de cache en RAM (default 2MB)
        PRAGMA temp_store = MEMORY;         -- Tablas temporales en RAM (no disco)
        PRAGMA mmap_size = 268435456;       -- 256MB memory-mapped I/O
        PRAGMA page_size = 4096;            -- 4KB paginas (optimo para SSD)
        PRAGMA auto_vacuum = INCREMENTAL;   -- Recuperar espacio sin bloquear
        PRAGMA busy_timeout = 5000;         -- 5s timeout antes de error BUSY
        PRAGMA optimize;                    -- Optimizar automaticamente
    ";
    cmd.ExecuteNonQuery();

    optionsBuilder
        .UseSqlite(conn)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking); // Por defecto sin tracking
}

// === Indices estrategicos para consultas frecuentes ===
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Sessions: busqueda por estado activo (la consulta mas frecuente)
    modelBuilder.Entity<Session>()
        .HasIndex(s => new { s.Status, s.ComputerId })
        .HasFilter("[Status] = 'Active'")  // Indice parcial: solo sesiones activas
        .HasDatabaseName("IX_Sessions_Active");

    // Sessions: reportes por fecha
    modelBuilder.Entity<Session>()
        .HasIndex(s => s.CreatedAt)
        .HasDatabaseName("IX_Sessions_Date");

    // Sales: reportes por fecha y empleado
    modelBuilder.Entity<Sale>()
        .HasIndex(s => new { s.CreatedAt, s.EmployeeId })
        .HasDatabaseName("IX_Sales_Date_Employee");

    // Products: busqueda por nombre y categoria (POS)
    modelBuilder.Entity<Product>()
        .HasIndex(p => new { p.Category, p.Name })
        .HasFilter("[IsActive] = 1")
        .HasDatabaseName("IX_Products_Active");

    // Clients: busqueda por codigo y nombre
    modelBuilder.Entity<Client>()
        .HasIndex(c => c.Code)
        .IsUnique()
        .HasDatabaseName("IX_Clients_Code");

    modelBuilder.Entity<Client>()
        .HasIndex(c => c.Name)
        .HasDatabaseName("IX_Clients_Name");

    // Computers: estado actual
    modelBuilder.Entity<Computer>()
        .HasIndex(c => c.Status)
        .HasDatabaseName("IX_Computers_Status");
}
```

### 7.2 Compiled Queries (EF Core)

```csharp
// === Consultas pre-compiladas para las operaciones mas frecuentes ===
// Se compilan una sola vez y se reutilizan (evita overhead de traduccion SQL)

public static class CompiledQueries
{
    // Obtener todas las sesiones activas (ejecutada cada segundo por el timer)
    public static readonly Func<AppDbContext, IAsyncEnumerable<Session>>
        GetActiveSessions = EF.CompileAsyncQuery(
            (AppDbContext db) => db.Sessions
                .Include(s => s.Computer)
                .Include(s => s.Client)
                .Where(s => s.Status == SessionStatus.Active)
                .OrderBy(s => s.PlannedEnd));

    // Obtener computadora por ID con sesion activa
    public static readonly Func<AppDbContext, int, Task<Computer?>>
        GetComputerWithActiveSession = EF.CompileAsyncQuery(
            (AppDbContext db, int id) => db.Computers
                .Include(c => c.Sessions.Where(s => s.Status == SessionStatus.Active))
                .FirstOrDefault(c => c.Id == id));

    // Obtener productos activos por categoria (POS)
    public static readonly Func<AppDbContext, string, IAsyncEnumerable<Product>>
        GetProductsByCategory = EF.CompileAsyncQuery(
            (AppDbContext db, string category) => db.Products
                .Where(p => p.IsActive && p.Category == category)
                .OrderBy(p => p.Name));

    // Ventas del dia (dashboard)
    public static readonly Func<AppDbContext, DateTime, Task<decimal>>
        GetTodaySalesTotal = EF.CompileAsyncQuery(
            (AppDbContext db, DateTime today) => db.Sales
                .Where(s => s.CreatedAt >= today)
                .Sum(s => s.Total));

    // Buscar cliente por codigo o nombre (AutoSuggestBox)
    public static readonly Func<AppDbContext, string, IAsyncEnumerable<Client>>
        SearchClients = EF.CompileAsyncQuery(
            (AppDbContext db, string term) => db.Clients
                .Where(c => c.IsActive &&
                    (c.Code.Contains(term) || c.Name.Contains(term)))
                .Take(10)
                .OrderBy(c => c.Name));
}
```

### 7.3 Cache Multinivel

```csharp
// === CacheService.cs - Sistema de cache de 3 niveles ===

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly AppDbContext _db;

    // NIVEL 1: Cache en diccionario estatico (0ms acceso, datos que casi nunca cambian)
    private static readonly ConcurrentDictionary<string, object> _staticCache = new();

    // NIVEL 2: MemoryCache con expiracion (< 1ms acceso)
    // NIVEL 3: SQLite (< 5ms acceso local)

    // === Configuracion de tarifas (cambia rara vez) ===
    public async Task<List<RateConfig>> GetRatesAsync()
    {
        return (List<RateConfig>)_staticCache.GetOrAdd("rates",
            _ => _db.RateConfigs.AsNoTracking().ToList());
    }

    public void InvalidateRates() => _staticCache.TryRemove("rates", out _);

    // === Productos del POS (cambia poco, se consulta mucho) ===
    public async Task<List<Product>> GetActiveProductsAsync()
    {
        return await _cache.GetOrCreateAsync("products_active", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            entry.Priority = CacheItemPriority.High;
            return await _db.Products
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();
        });
    }

    // === Dashboard KPIs (cambia frecuentemente, cache corto) ===
    public async Task<DashboardDto> GetDashboardAsync()
    {
        return await _cache.GetOrCreateAsync("dashboard", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
            var today = DateTime.Today;
            return new DashboardDto
            {
                TotalRevenue = await CompiledQueries.GetTodaySalesTotal(_db, today),
                ActiveSessions = await _db.Sessions.CountAsync(s => s.Status == SessionStatus.Active),
                AvailablePCs = await _db.Computers.CountAsync(c => c.Status == ComputerStatus.Available),
                TotalPCs = await _db.Computers.CountAsync(),
                PrintJobsToday = await _db.PrintJobs.CountAsync(p => p.CreatedAt >= today)
            };
        });
    }

    // === Invalidacion selectiva cuando hay cambios ===
    public void InvalidateProducts() => _cache.Remove("products_active");
    public void InvalidateDashboard() => _cache.Remove("dashboard");
}
```

### 7.4 UI - Rendimiento de WinUI 3

```xml
<!-- === ComputerMapPage.xaml - GridView virtualizado de PCs === -->

<!-- ItemsRepeater con virtualizacion automatica (solo renderiza lo visible) -->
<ScrollViewer>
    <ItemsRepeater
        ItemsSource="{x:Bind ViewModel.Computers, Mode=OneWay}"
        Layout="{StaticResource ComputerGridLayout}">

        <!-- x:Bind compilado: 10x mas rapido que Binding clasico -->
        <ItemsRepeater.ItemTemplate>
            <DataTemplate x:DataType="models:ComputerViewModel">
                <!-- x:Load: solo carga en memoria cuando es visible -->
                <local:ComputerCard
                    x:Load="{x:Bind IsVisible, Mode=OneWay}"
                    ComputerName="{x:Bind Name}"
                    Status="{x:Bind Status, Mode=OneWay}"
                    TimeRemaining="{x:Bind TimeRemaining, Mode=OneWay}"
                    ClientName="{x:Bind ClientName, Mode=OneWay}"
                    Progress="{x:Bind Progress, Mode=OneWay}"
                    Command="{x:Bind SelectCommand}" />
            </DataTemplate>
        </ItemsRepeater.ItemTemplate>
    </ItemsRepeater>
</ScrollViewer>

<!-- Layout responsivo: se adapta al ancho disponible -->
<Page.Resources>
    <UniformGridLayout
        x:Key="ComputerGridLayout"
        MinItemWidth="200"
        MinItemHeight="180"
        MinRowSpacing="12"
        MinColumnSpacing="12"
        ItemsStretch="Fill" />
</Page.Resources>
```

```csharp
// === TimerViewModel.cs - Actualizacion eficiente del timer ===

public partial class TimerViewModel : ObservableObject
{
    private readonly PeriodicTimer _timer = new(TimeSpan.FromSeconds(1));
    private readonly CancellationTokenSource _cts = new();

    // ObservableProperty via source generator (cero reflection)
    [ObservableProperty]
    private ObservableCollection<SessionViewModel> _activeSessions = new();

    public async Task StartTimerLoopAsync()
    {
        while (await _timer.WaitForNextTickAsync(_cts.Token))
        {
            // Actualizar solo las sesiones que cambiaron (no toda la lista)
            foreach (var session in ActiveSessions)
            {
                session.UpdateCountdown(); // Solo actualiza TimeRemaining y Progress
            }

            // Verificar expiraciones (solo las proximas a expirar)
            var expiring = ActiveSessions
                .Where(s => s.TimeRemaining <= TimeSpan.Zero)
                .ToList();

            foreach (var session in expiring)
            {
                await ExpireSessionAsync(session);
            }
        }
    }
}

// === SessionViewModel.cs - Actualizacion granular sin re-render completo ===

public partial class SessionViewModel : ObservableObject
{
    [ObservableProperty] private TimeSpan _timeRemaining;
    [ObservableProperty] private double _progress;     // 0.0 - 1.0
    [ObservableProperty] private string _statusColor;  // Para el binding de color

    public void UpdateCountdown()
    {
        if (IsPaused) return;

        TimeRemaining = PlannedEnd - DateTime.Now;
        Progress = 1.0 - (TimeRemaining.TotalSeconds / TotalDuration.TotalSeconds);

        // Cambiar color solo cuando cruza un umbral (no cada segundo)
        var newColor = TimeRemaining.TotalMinutes switch
        {
            <= 1 => "Error",       // Rojo: menos de 1 min
            <= 5 => "Warning",     // Ambar: menos de 5 min
            _    => "Success"      // Verde: normal
        };

        if (newColor != StatusColor)
            StatusColor = newColor;
    }
}
```

### 7.5 SignalR - Comunicacion Optimizada

```csharp
// === SignalR con MessagePack (50% menos ancho de banda que JSON) ===

// Servidor
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = false;  // Produccion
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(60);
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB max (screenshots)
    options.StreamBufferCapacity = 20;
})
.AddMessagePackProtocol(options =>
{
    options.SerializerOptions = MessagePackSerializerOptions.Standard
        .WithCompression(MessagePackCompression.Lz4BlockArray); // Compresion LZ4
});

// === Batch updates: actualizar 50 PCs en 1 mensaje (no 50 mensajes) ===

public class CafeHub : Hub<IAgentHub>
{
    // Enviar actualizacion de timer a TODOS los agentes en una sola llamada
    public async Task BroadcastTimerUpdates(List<TimerUpdateDto> updates)
    {
        // Un solo mensaje con todas las actualizaciones
        await Clients.All.BatchUpdateTimers(updates);
    }

    // Enviar solo a un agente especifico (por connection ID)
    public async Task LockComputer(string connectionId)
    {
        await Clients.Client(connectionId).LockScreen();
    }
}

// === Agente: reconexion automatica con backoff exponencial ===

public class SignalRClientService : IAsyncDisposable
{
    private HubConnection _connection;

    public async Task ConnectAsync()
    {
        _connection = new HubConnectionBuilder()
            .WithUrl($"http://{_serverIp}:5000/cafehub")
            .AddMessagePackProtocol(opt =>
                opt.SerializerOptions = MessagePackSerializerOptions.Standard
                    .WithCompression(MessagePackCompression.Lz4BlockArray))
            .WithAutomaticReconnect(new RetryPolicy()) // Backoff exponencial
            .Build();

        _connection.Reconnecting += _ =>
        {
            // Mostrar "Reconectando..." en overlay del agente
            _lockService.ShowReconnecting();
            return Task.CompletedTask;
        };

        _connection.Reconnected += _ =>
        {
            // Re-registrar PC despues de reconexion
            return _connection.InvokeAsync("Register", GetRegistrationInfo());
        };

        await _connection.StartAsync();
    }

    // Backoff: 0s, 2s, 5s, 10s, 30s, 60s (max)
    private class RetryPolicy : IRetryPolicy
    {
        private static readonly TimeSpan[] _delays = {
            TimeSpan.Zero,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(60)
        };

        public TimeSpan? NextRetryDelay(RetryContext ctx) =>
            ctx.PreviousRetryCount < _delays.Length
                ? _delays[ctx.PreviousRetryCount]
                : TimeSpan.FromSeconds(60);
    }
}
```

### 7.6 Startup Optimizado

```csharp
// === App.xaml.cs - Startup del servidor optimizado ===

public partial class App : Application
{
    // 1. Registrar servicios en paralelo donde sea posible
    private static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Singleton: una sola instancia para toda la app
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<INotificationService, NotificationService>();

        // Scoped: nueva instancia por operacion
        services.AddDbContext<AppDbContext>(ServiceLifetime.Transient);

        // Transient: nueva instancia cada vez (ViewModels ligeros)
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<ComputerMapViewModel>();

        // MemoryCache con limites
        services.AddMemoryCache(options =>
        {
            options.SizeLimit = 1024;                           // Max 1024 items
            options.CompactionPercentage = 0.25;                // Compactar 25% al llegar al limite
            options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
        });

        // Serilog: logging asincrono (no bloquea UI)
        services.AddLogging(builder =>
        {
            builder.AddSerilog(new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Async(a => a.File(
                    path: "logs/desktopcafe-.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 30,
                    buffered: true,                            // Buffered para rendimiento
                    flushToDiskInterval: TimeSpan.FromSeconds(5)))
                .CreateLogger());
        });

        return services.BuildServiceProvider();
    }

    // 2. Precarga de datos criticos en background (no bloquea UI)
    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        MainWindow = new MainWindow();
        MainWindow.Activate();

        // Precargar datos en paralelo DESPUES de mostrar la ventana
        _ = Task.Run(async () =>
        {
            var cache = Services.GetRequiredService<ICacheService>();
            await Task.WhenAll(
                cache.GetRatesAsync(),           // Tarifas
                cache.GetActiveProductsAsync(),   // Productos POS
                cache.GetDashboardAsync()         // KPIs
            );
        });
    }
}
```

```xml
<!-- === .csproj - Compilacion optimizada === -->
<PropertyGroup>
    <TargetFramework>net9.0-windows10.0.22621.0</TargetFramework>
    <PublishReadyToRun>true</PublishReadyToRun>         <!-- Pre-compilar JIT = startup rapido -->
    <PublishTrimmed>false</PublishTrimmed>               <!-- No trimming (WinUI 3 no compatible aun) -->
    <TieredCompilation>true</TieredCompilation>         <!-- JIT progresivo -->
    <TieredPGO>true</TieredPGO>                         <!-- Profile-Guided Optimization -->
    <ServerGarbageCollection>true</ServerGarbageCollection> <!-- GC optimizado -->
    <InvariantGlobalization>false</InvariantGlobalization>
</PropertyGroup>
```

### 7.7 Memoria - Reduccion de Allocations

```csharp
// === Usar Span<T> y stackalloc para operaciones frecuentes ===

// Generar codigo de cliente (6 caracteres alfanumericos)
// ANTES: string.Concat + new char[] + allocations
// DESPUES: stackalloc, cero allocations en heap
public static string GenerateClientCode()
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    Span<char> buffer = stackalloc char[6];

    for (int i = 0; i < buffer.Length; i++)
        buffer[i] = chars[Random.Shared.Next(chars.Length)];

    return new string(buffer);
}

// === Generar codigo de voucher WiFi (8 caracteres) ===
public static string GenerateVoucherCode()
{
    Span<char> buffer = stackalloc char[8];
    const string chars = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ"; // Sin 0,O,1,I,L

    for (int i = 0; i < buffer.Length; i++)
        buffer[i] = chars[Random.Shared.Next(chars.Length)];

    return new string(buffer);
}

// === Reutilizar StringBuilder para formateo de recibos ===
public class ReceiptBuilder : IDisposable
{
    private static readonly ObjectPool<StringBuilder> _pool =
        new DefaultObjectPoolProvider().CreateStringBuilderPool(
            initialCapacity: 512, maximumRetainedCapacity: 4096);

    private readonly StringBuilder _sb;

    public ReceiptBuilder() => _sb = _pool.Get();

    public ReceiptBuilder AddHeader(string cafeName)
    {
        _sb.AppendLine("================================");
        _sb.AppendLine($"  {cafeName}");
        _sb.AppendLine("================================");
        return this;
    }

    public ReceiptBuilder AddItem(string name, int qty, decimal price)
    {
        _sb.AppendLine($"  {name,-20} {qty}x  ${price:F2}");
        return this;
    }

    public string Build() => _sb.ToString();
    public void Dispose() => _pool.Return(_sb);
}
```

### 7.8 Base de Datos - Bulk Operations y Reportes

```csharp
// === Reportes optimizados: una sola query SQL para el reporte completo ===

public class ReportRepository : IReportRepository
{
    // Reporte diario: UNA query que trae todo (no N+1 queries)
    public async Task<DailyReportDto> GetDailyReportAsync(DateTime date)
    {
        var nextDay = date.AddDays(1);

        // Ejecutar todas las consultas en paralelo (SQLite WAL permite lecturas concurrentes)
        var revenueTask = _db.Database.SqlQueryRaw<RevenueBreakdown>(@"
            SELECT
                COALESCE(SUM(CASE WHEN s.SessionType IS NOT NULL THEN s.TotalCharge END), 0) as SessionRevenue,
                COALESCE(SUM(CASE WHEN sl.Id IS NOT NULL THEN sl.Total END), 0) as SalesRevenue,
                COALESCE(SUM(CASE WHEN pj.Id IS NOT NULL THEN pj.Cost END), 0) as PrintRevenue
            FROM Sessions s
            LEFT JOIN Sales sl ON sl.CreatedAt >= @p0 AND sl.CreatedAt < @p1
            LEFT JOIN PrintJobs pj ON pj.CreatedAt >= @p0 AND pj.CreatedAt < @p1
            WHERE s.CreatedAt >= @p0 AND s.CreatedAt < @p1",
            date, nextDay).FirstOrDefaultAsync();

        var hourlyTask = _db.Database.SqlQueryRaw<HourlyUsage>(@"
            SELECT
                CAST(strftime('%H', StartTime) AS INTEGER) as Hour,
                COUNT(*) as SessionCount,
                ROUND(AVG(julianday(COALESCE(EndTime, datetime('now'))) - julianday(StartTime)) * 24 * 60, 0) as AvgMinutes
            FROM Sessions
            WHERE CreatedAt >= @p0 AND CreatedAt < @p1
            GROUP BY CAST(strftime('%H', StartTime) AS INTEGER)
            ORDER BY Hour",
            date, nextDay).ToListAsync();

        var topProductsTask = _db.Database.SqlQueryRaw<TopProduct>(@"
            SELECT p.Name, SUM(si.Quantity) as TotalQty, SUM(si.Subtotal) as TotalRevenue
            FROM SaleItems si
            JOIN Products p ON p.Id = si.ProductId
            JOIN Sales s ON s.Id = si.SaleId
            WHERE s.CreatedAt >= @p0 AND s.CreatedAt < @p1
            GROUP BY p.Id
            ORDER BY TotalRevenue DESC
            LIMIT 10",
            date, nextDay).ToListAsync();

        // Esperar todas en paralelo
        await Task.WhenAll(revenueTask, hourlyTask, topProductsTask);

        return new DailyReportDto
        {
            Revenue = await revenueTask,
            HourlyUsage = await hourlyTask,
            TopProducts = await topProductsTask
        };
    }
}

// === Cleanup automatico de datos antiguos (no dejar crecer la BD infinitamente) ===

public class MaintenanceService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(24), ct); // Ejecutar cada 24 horas

            using var db = _factory.CreateDbContext();

            // Eliminar logs de auditoria > 90 dias
            var cutoff = DateTime.Now.AddDays(-90);
            await db.Database.ExecuteSqlRawAsync(
                "DELETE FROM AuditLogs WHERE CreatedAt < @p0", cutoff);

            // Eliminar vouchers WiFi expirados > 30 dias
            var voucherCutoff = DateTime.Now.AddDays(-30);
            await db.Database.ExecuteSqlRawAsync(
                "DELETE FROM WiFiVouchers WHERE ExpiresAt < @p0", voucherCutoff);

            // Optimizar base de datos
            await db.Database.ExecuteSqlRawAsync("PRAGMA incremental_vacuum;");
            await db.Database.ExecuteSqlRawAsync("PRAGMA optimize;");

            _logger.LogInformation("Mantenimiento de BD completado");
        }
    }
}
```

### 7.9 Backup Incremental Automatico

```csharp
// === BackupService.cs - Backup sin bloquear la aplicacion ===

public class BackupService : BackgroundService
{
    private readonly string _backupDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Desktopcafe", "Backups");

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        Directory.CreateDirectory(_backupDir);

        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(6), ct); // Cada 6 horas

            try
            {
                var backupFile = Path.Combine(_backupDir,
                    $"desktopcafe_{DateTime.Now:yyyyMMdd_HHmmss}.db");

                // SQLite Online Backup API: copia sin bloquear lecturas/escrituras
                using var source = new SqliteConnection("Data Source=desktopcafe.db");
                using var backup = new SqliteConnection($"Data Source={backupFile}");
                await source.OpenAsync(ct);
                await backup.OpenAsync(ct);
                source.BackupDatabase(backup);

                // Limpiar backups antiguos (mantener 7 dias)
                CleanOldBackups(maxAgeDays: 7);

                _logger.LogInformation("Backup creado: {File}", backupFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en backup automatico");
            }
        }
    }

    private void CleanOldBackups(int maxAgeDays)
    {
        var cutoff = DateTime.Now.AddDays(-maxAgeDays);
        foreach (var file in Directory.GetFiles(_backupDir, "*.db"))
        {
            if (File.GetCreationTime(file) < cutoff)
                File.Delete(file);
        }
    }
}
```

### 7.10 Profiling y Monitoreo en Produccion

```csharp
// === PerformanceMonitor.cs - Metricas internas del servidor ===

public class PerformanceMonitor
{
    private static readonly Stopwatch _uptime = Stopwatch.StartNew();

    // Metricas visibles en Settings > Diagnosticos
    public ServerMetrics GetMetrics() => new()
    {
        Uptime = _uptime.Elapsed,
        MemoryUsageMB = Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024,
        ThreadCount = Process.GetCurrentProcess().Threads.Count,
        GCGen0Collections = GC.CollectionCount(0),
        GCGen1Collections = GC.CollectionCount(1),
        GCGen2Collections = GC.CollectionCount(2),
        GCTotalMemory = GC.GetTotalMemory(false) / 1024 / 1024,
        ConnectedAgents = _signalRService.ConnectedCount,
        ActiveSessions = _sessionService.ActiveCount,
        DbSizeMB = new FileInfo("desktopcafe.db").Length / 1024 / 1024,
        CacheHitRate = _cacheService.HitRate
    };
}

// Visible en la pagina de Settings:
// ┌──────────────────────────────────────────────────┐
// │  Diagnosticos del Sistema                         │
// │                                                    │
// │  Uptime:           12h 34m 56s                    │
// │  Memoria:          87 MB / 150 MB target          │
// │  Hilos activos:    12                              │
// │  Agentes online:   15/15                           │
// │  Sesiones activas: 8                               │
// │  BD:               45 MB                           │
// │  Cache hit rate:   94.2%                           │
// │  GC Gen0/1/2:      234 / 45 / 3                   │
// │                                                    │
// │  [Forzar GC]  [Exportar logs]  [Backup ahora]    │
// └──────────────────────────────────────────────────┘
```

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
