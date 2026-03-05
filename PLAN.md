# Desktopcafe - Plan de Desarrollo

## Software de Gestion de Cibercafe / Centro de Internet

**Stack:** C# .NET 8 + WPF (Windows Presentation Foundation)
**Arquitectura:** Desktop con servidor local (SQLite)
**Alcance:** Una sola ubicacion, todos los modulos

---

## 1. Arquitectura General

```
┌─────────────────────────────────────────────────────┐
│                  PC SERVIDOR (Admin)                │
│  ┌─────────────┐  ┌──────────┐  ┌───────────────┐  │
│  │  WPF UI     │  │ REST API │  │  SQLite DB    │  │
│  │  (Admin     │  │ (local)  │  │               │  │
│  │   Panel)    │  │ Kestrel  │  │  - Sesiones   │  │
│  │             │  │          │  │  - Clientes   │  │
│  │ - Dashboard │  │ :5000    │  │  - Productos  │  │
│  │ - Timer     │  │          │  │  - Ventas     │  │
│  │ - POS       │  │          │  │  - Empleados  │  │
│  │ - Reportes  │  │          │  │  - Config     │  │
│  └─────────────┘  └──────────┘  └───────────────┘  │
│         │              │                            │
│         └──────────────┘                            │
│                  │  Red Local (LAN)                  │
└──────────────────┼──────────────────────────────────┘
                   │
      ┌────────────┼────────────┐
      │            │            │
 ┌────▼───┐  ┌────▼───┐  ┌────▼───┐
 │ PC #1  │  │ PC #2  │  │ PC #N  │
 │ Cliente│  │ Cliente│  │ Cliente│
 │ Agent  │  │ Agent  │  │ Agent  │
 └────────┘  └────────┘  └────────┘
```

### Componentes:
1. **Desktopcafe.Server** - Aplicacion WPF del servidor/admin
2. **Desktopcafe.Agent** - Servicio Windows que corre en cada PC cliente
3. **Desktopcafe.Core** - Libreria compartida (modelos, DTOs, utilidades)
4. **Desktopcafe.Data** - Capa de acceso a datos (Entity Framework + SQLite)

---

## 2. Estructura del Proyecto

```
Desktopcafe/
├── src/
│   ├── Desktopcafe.sln
│   │
│   ├── Desktopcafe.Core/              # Libreria compartida
│   │   ├── Models/
│   │   │   ├── Client.cs              # Modelo de cliente/usuario
│   │   │   ├── Computer.cs            # Modelo de computadora
│   │   │   ├── Session.cs             # Sesion activa (timer)
│   │   │   ├── Product.cs             # Producto del inventario
│   │   │   ├── Sale.cs                # Venta/transaccion
│   │   │   ├── SaleItem.cs            # Detalle de venta
│   │   │   ├── Employee.cs            # Empleado
│   │   │   ├── PrintJob.cs            # Trabajo de impresion
│   │   │   ├── Reservation.cs         # Reservacion
│   │   │   └── Shift.cs               # Turno de empleado
│   │   ├── DTOs/
│   │   ├── Enums/
│   │   │   ├── SessionType.cs         # Prepago, Postpago, Libre
│   │   │   ├── ComputerStatus.cs      # Disponible, Ocupada, Mantenimiento
│   │   │   └── PaymentMethod.cs       # Efectivo, Tarjeta, Transferencia
│   │   └── Interfaces/
│   │
│   ├── Desktopcafe.Data/              # Capa de datos
│   │   ├── AppDbContext.cs
│   │   ├── Migrations/
│   │   └── Repositories/
│   │       ├── ClientRepository.cs
│   │       ├── SessionRepository.cs
│   │       ├── ProductRepository.cs
│   │       ├── SaleRepository.cs
│   │       └── ReportRepository.cs
│   │
│   ├── Desktopcafe.Server/            # App WPF (Servidor/Admin)
│   │   ├── App.xaml
│   │   ├── MainWindow.xaml            # Ventana principal con navegacion
│   │   ├── Views/
│   │   │   ├── DashboardView.xaml     # Panel principal con resumen
│   │   │   ├── ComputerMapView.xaml   # Mapa visual de PCs
│   │   │   ├── TimerView.xaml         # Control de sesiones/timer
│   │   │   ├── POSView.xaml           # Punto de venta
│   │   │   ├── ClientsView.xaml       # Gestion de clientes
│   │   │   ├── PrinterView.xaml       # Gestion de impresoras
│   │   │   ├── InventoryView.xaml     # Inventario de productos
│   │   │   ├── ReportsView.xaml       # Reportes y estadisticas
│   │   │   ├── EmployeesView.xaml     # Gestion de empleados
│   │   │   ├── ReservationsView.xaml  # Reservaciones
│   │   │   ├── WiFiView.xaml          # Control de WiFi
│   │   │   ├── GamingView.xaml        # Centro de gaming
│   │   │   └── SettingsView.xaml      # Configuracion general
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs
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
│   │   │   └── SettingsViewModel.cs
│   │   ├── Services/
│   │   │   ├── NetworkService.cs      # Comunicacion con agentes
│   │   │   ├── PrinterService.cs      # Control de impresoras
│   │   │   ├── TimerService.cs        # Logica del temporizador
│   │   │   ├── BillingService.cs      # Calculo de cobros
│   │   │   ├── WiFiService.cs         # Control de acceso WiFi
│   │   │   └── NotificationService.cs # Alertas y notificaciones
│   │   ├── Controls/
│   │   │   ├── ComputerCard.xaml      # Tarjeta visual de PC
│   │   │   ├── TimerDisplay.xaml      # Display del temporizador
│   │   │   └── SessionDialog.xaml     # Dialogo para nueva sesion
│   │   └── Themes/
│   │       ├── Colors.xaml
│   │       ├── Styles.xaml
│   │       └── DarkTheme.xaml
│   │
│   └── Desktopcafe.Agent/             # Servicio en PCs cliente
│       ├── Program.cs
│       ├── AgentService.cs            # Servicio Windows
│       ├── ScreenLocker.cs            # Bloqueo de pantalla
│       ├── ProcessMonitor.cs          # Monitor de procesos
│       ├── PrintInterceptor.cs        # Interceptor de impresion
│       └── ClientUI/
│           ├── LockScreen.xaml        # Pantalla de bloqueo
│           └── SessionInfo.xaml       # Info de sesion para el usuario
│
├── tests/
│   ├── Desktopcafe.Core.Tests/
│   ├── Desktopcafe.Data.Tests/
│   └── Desktopcafe.Server.Tests/
│
├── PLAN.md
├── .gitignore
└── LICENSE
```

---

## 3. Modulos Detallados

### 3.1 Timer / Control de Tiempo (Modulo Principal)
**Prioridad: CRITICA**

Funcionalidades:
- Iniciar sesion: Prepago (paga antes), Postpago (paga al salir), Libre (sin limite)
- Temporizador en tiempo real con countdown visible
- Alertas automaticas: 5 min, 2 min, 1 min antes de terminar
- Extension de tiempo durante sesion activa
- Pausa de sesion (para ir al bano, etc.)
- Bloqueo automatico de PC al terminar tiempo
- Tarifas configurables: por hora, media hora, 15 min
- Tarifas diferenciadas: horario normal, nocturno, fin de semana
- Descuentos para clientes frecuentes

Flujo:
```
Cliente llega → Seleccionar PC → Elegir tipo de sesion →
Establecer tiempo/monto → Iniciar sesion → Timer activo →
Alerta pre-expiracion → Bloqueo al terminar → Cobro
```

### 3.2 Control de Impresoras
**Prioridad: ALTA**

Funcionalidades:
- Deteccion automatica de impresoras en red
- Cobro por pagina impresa (B/N y color con precios diferentes)
- Contador de paginas por sesion
- Cola de impresion visible en el panel admin
- Aprobar/rechazar trabajos de impresion desde el servidor
- Historial de impresiones por cliente
- Limite de paginas por sesion (configurable)
- Soporte para impresora de tickets (recibos)

### 3.3 Punto de Venta (POS)
**Prioridad: ALTA**

Funcionalidades:
- Catalogo de productos con categorias (bebidas, snacks, papeleria)
- Carrito de compras rapido
- Metodos de pago: efectivo, tarjeta, transferencia
- Calculo de cambio automatico
- Vincular ventas a sesion activa (cobro consolidado)
- Ticket/recibo impreso o digital
- Apertura/cierre de caja con arqueo
- Historial de ventas del dia

### 3.4 Control de WiFi
**Prioridad: MEDIA**

Funcionalidades:
- Generacion de vouchers WiFi con tiempo limitado
- Codigos QR para acceso rapido
- Ancho de banda configurable por voucher
- Monitoreo de dispositivos conectados
- Bloqueo de dispositivos
- Integracion con router (MikroTik API o similar)

### 3.5 Gaming Center
**Prioridad: MEDIA**

Funcionalidades:
- Lanzador de juegos instalados en cada PC
- Categorias de juegos (FPS, MOBA, RPG, etc.)
- Registro de juegos mas jugados
- Perfil de jugador con estadisticas
- Sistema basico de torneos (bracket simple)
- Tarifas especiales para gaming

### 3.6 Gestion de Clientes
**Prioridad: ALTA**

Funcionalidades:
- Registro rapido (nombre, telefono, correo)
- Tarjeta de cliente / codigo unico
- Historial de sesiones y compras
- Sistema de puntos/lealtad
- Saldo prepago (recargas)
- Clientes frecuentes con descuento automatico

### 3.7 Gestion de Empleados
**Prioridad: MEDIA**

Funcionalidades:
- Registro de empleados con roles (admin, cajero)
- Control de turnos (entrada/salida)
- Permisos por rol (quien puede dar descuentos, reembolsos, etc.)
- Registro de acciones (auditoria)
- Reporte de ventas por empleado

### 3.8 Inventario
**Prioridad: MEDIA**

Funcionalidades:
- Catalogo de productos con stock
- Alertas de stock bajo
- Registro de entradas (compras a proveedor)
- Registro de salidas (ventas)
- Historial de movimientos
- Reporte de productos mas vendidos

### 3.9 Reservaciones
**Prioridad: BAJA**

Funcionalidades:
- Reservar PC para horario especifico
- Reservar para grupos/eventos
- Notificacion de reserva proxima
- Cancelacion con politica configurable

### 3.10 Reportes y Estadisticas
**Prioridad: ALTA**

Funcionalidades:
- Dashboard en tiempo real:
  - PCs activas/disponibles
  - Ingresos del dia
  - Sesiones activas
  - Productos vendidos
- Reportes generables:
  - Ingresos diarios/semanales/mensuales
  - Horas pico de uso
  - Productos mas vendidos
  - Clientes mas frecuentes
  - Uso por PC (identificar PCs con problemas)
  - Impresiones totales y costo
- Exportar a Excel/PDF
- Graficas con LiveCharts2

---

## 4. Modelo de Base de Datos

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  Computers   │     │   Sessions   │     │   Clients    │
├──────────────┤     ├──────────────┤     ├──────────────┤
│ Id           │◄────│ ComputerId   │     │ Id           │
│ Name         │     │ ClientId     │────►│ Name         │
│ IpAddress    │     │ StartTime    │     │ Phone        │
│ MacAddress   │     │ EndTime      │     │ Email        │
│ Status       │     │ Duration     │     │ Balance      │
│ Zone         │     │ SessionType  │     │ Points       │
│ IsGaming     │     │ RatePerHour  │     │ CreatedAt    │
│ Specs        │     │ TotalCharge  │     └──────────────┘
└──────────────┘     │ IsPaused     │
                     │ Status       │     ┌──────────────┐
                     └──────────────┘     │  Employees   │
                                          ├──────────────┤
┌──────────────┐     ┌──────────────┐     │ Id           │
│  Products    │     │    Sales     │     │ Name         │
├──────────────┤     ├──────────────┤     │ Username     │
│ Id           │     │ Id           │     │ PasswordHash │
│ Name         │     │ EmployeeId   │────►│ Role         │
│ Category     │     │ ClientId     │     │ IsActive     │
│ Price        │     │ SessionId    │     └──────────────┘
│ Stock        │     │ Total        │
│ MinStock     │     │ PaymentMethod│     ┌──────────────┐
│ ImagePath    │     │ CreatedAt    │     │  PrintJobs   │
└──────────────┘     └──────────────┘     ├──────────────┤
      │                    │              │ Id           │
      │              ┌─────▼──────┐       │ SessionId    │
      │              │ SaleItems  │       │ ComputerId   │
      └─────────────►├────────────┤       │ Pages        │
                     │ SaleId     │       │ IsColor      │
                     │ ProductId  │       │ Cost         │
                     │ Quantity   │       │ Status       │
                     │ UnitPrice  │       │ PrinterName  │
                     │ Subtotal   │       │ CreatedAt    │
                     └────────────┘       └──────────────┘

┌──────────────┐     ┌──────────────┐
│ Reservations │     │   Shifts     │
├──────────────┤     ├──────────────┤
│ Id           │     │ Id           │
│ ComputerId   │     │ EmployeeId   │
│ ClientId     │     │ StartTime    │
│ StartTime    │     │ EndTime      │
│ Duration     │     │ CashStart    │
│ Status       │     │ CashEnd      │
│ Notes        │     │ Notes        │
└──────────────┘     └──────────────┘
```

---

## 5. Tecnologias y Librerias

| Componente | Tecnologia |
|---|---|
| Framework | .NET 8 (LTS) |
| UI Servidor | WPF con Material Design (MaterialDesignInXaml) |
| UI Agente | WPF (pantalla de bloqueo) |
| Patron UI | MVVM con CommunityToolkit.Mvvm |
| Base de Datos | SQLite via Entity Framework Core |
| Comunicacion | SignalR (servidor-agente en tiempo real) |
| Graficas | LiveCharts2 |
| Reportes PDF | QuestPDF |
| Reportes Excel | ClosedXML |
| Impresoras | System.Drawing.Printing + Win32 API |
| WiFi/Red | Manageable via netsh / MikroTik API |
| Iconos | Material Design Icons |
| Instalador | Inno Setup |
| Logging | Serilog |
| DI | Microsoft.Extensions.DependencyInjection |
| Tests | xUnit + Moq |

---

## 6. Comunicacion Servidor-Agente

Protocolo: **SignalR** sobre la red local (LAN)

### Servidor → Agente:
- `LockScreen` - Bloquear pantalla (sesion terminada)
- `UnlockScreen` - Desbloquear (sesion iniciada)
- `ShowMessage` - Mostrar mensaje al usuario
- `ShowTimeWarning` - Alerta de tiempo restante
- `Shutdown` - Apagar PC
- `Restart` - Reiniciar PC
- `GetScreenshot` - Captura de pantalla remota
- `UpdateConfig` - Actualizar configuracion

### Agente → Servidor:
- `Register` - Registrar PC al iniciar
- `Heartbeat` - Cada 30 seg (status, uso CPU/RAM)
- `PrintJobCreated` - Nuevo trabajo de impresion detectado
- `SessionRequest` - Cliente solicita sesion desde la PC
- `AlertAdmin` - Alerta al admin (error, hardware, etc.)

---

## 7. Fases de Desarrollo

### FASE 1: Fundamentos (Semanas 1-2)
- [ ] Crear solucion .NET con los 4 proyectos
- [ ] Configurar Entity Framework + SQLite + migraciones
- [ ] Implementar modelos de datos base
- [ ] Crear ventana principal WPF con navegacion lateral
- [ ] Implementar tema visual (Material Design)
- [ ] Sistema de autenticacion (login de empleado)
- [ ] CRUD de Computadoras (registro de PCs)

### FASE 2: Timer y Agente (Semanas 3-4)
- [ ] Desarrollar el Agente (servicio Windows)
- [ ] Pantalla de bloqueo del agente
- [ ] Comunicacion SignalR servidor-agente
- [ ] Mapa visual de computadoras (ComputerMapView)
- [ ] Iniciar/pausar/extender/terminar sesiones
- [ ] Temporizador en tiempo real con countdown
- [ ] Bloqueo/desbloqueo automatico
- [ ] Tarifas configurables

### FASE 3: POS e Impresoras (Semanas 5-6)
- [ ] CRUD de productos con categorias
- [ ] Interfaz de punto de venta
- [ ] Carrito y cobro
- [ ] Apertura/cierre de caja
- [ ] Deteccion de impresoras
- [ ] Interceptor de impresion en el agente
- [ ] Cobro por pagina impresa
- [ ] Impresion de tickets/recibos

### FASE 4: Clientes y Empleados (Semana 7)
- [ ] CRUD de clientes
- [ ] Sistema de saldo prepago
- [ ] Puntos de lealtad
- [ ] CRUD de empleados con roles
- [ ] Control de turnos
- [ ] Log de auditoria

### FASE 5: Inventario, WiFi y Gaming (Semanas 8-9)
- [ ] Control de inventario con alertas
- [ ] Generacion de vouchers WiFi
- [ ] Integracion basica con router
- [ ] Catalogo de juegos
- [ ] Lanzador de juegos en el agente
- [ ] Sistema de reservaciones

### FASE 6: Reportes y Pulido (Semanas 10-11)
- [ ] Dashboard con metricas en tiempo real
- [ ] Reportes de ingresos y uso
- [ ] Graficas con LiveCharts2
- [ ] Exportar a PDF y Excel
- [ ] Tema oscuro
- [ ] Manejo de errores y logging

### FASE 7: Testing y Despliegue (Semana 12)
- [ ] Tests unitarios para servicios criticos
- [ ] Tests de integracion
- [ ] Crear instalador (Inno Setup)
- [ ] Documentacion de usuario
- [ ] Configuracion inicial (wizard de primer uso)

---

## 8. Interfaz de Usuario - Mockup

```
┌─────────────────────────────────────────────────────────────┐
│  ☰  Desktopcafe                          Admin ▼  ⚙  🔔   │
├────────────┬────────────────────────────────────────────────┤
│            │                                                │
│ Dashboard  │  ┌─────────┐ ┌─────────┐ ┌─────────┐         │
│            │  │ Ingresos│ │ Sesiones│ │   PCs   │         │
│ Mapa PCs   │  │  $1,250 │ │   12    │ │  8/15   │         │
│            │  │  Hoy    │ │ Activas │ │ En uso  │         │
│ Timer      │  └─────────┘ └─────────┘ └─────────┘         │
│            │                                                │
│ POS        │  Mapa de Computadoras                         │
│            │  ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐          │
│ Clientes   │  │PC01│ │PC02│ │PC03│ │PC04│ │PC05│          │
│            │  │ ██ │ │ ░░ │ │ ██ │ │ ░░ │ │ ██ │          │
│ Impresoras │  │0:45│ │FREE│ │1:20│ │FREE│ │0:10│          │
│            │  └────┘ └────┘ └────┘ └────┘ └────┘          │
│ Inventario │  ┌────┐ ┌────┐ ┌────┐ ┌────┐ ┌────┐          │
│            │  │PC06│ │PC07│ │PC08│ │PC09│ │PC10│          │
│ Empleados  │  │ ░░ │ │ ▓▓ │ │ ██ │ │ ░░ │ │ ░░ │          │
│            │  │FREE│ │MANT│ │2:00│ │FREE│ │FREE│          │
│ Reportes   │  └────┘ └────┘ └────┘ └────┘ └────┘          │
│            │                                                │
│ WiFi       │  ██ = En uso  ░░ = Libre  ▓▓ = Mantenimiento  │
│            │                                                │
│ Gaming     │  Ultimas ventas          Alertas               │
│            │  ┌──────────────────┐   ┌──────────────────┐  │
│ Reservas   │  │ Coca-Cola  $15   │   │ PC03: 10 min     │  │
│            │  │ Impresion  $5    │   │ PC05: 2 min !!   │  │
│ Config     │  │ 1 hora PC  $20  │   │ Stock bajo: Agua  │  │
│            │  └──────────────────┘   └──────────────────┘  │
├────────────┴────────────────────────────────────────────────┤
│  Turno: Juan Perez | Caja: $3,450 | 15 PCs | 08:00-16:00  │
└─────────────────────────────────────────────────────────────┘
```

---

## 9. Seguridad

- Contrasenas hasheadas con BCrypt
- Roles y permisos por empleado
- Bloqueo de sesion de admin por inactividad
- El agente corre como servicio Windows (no puede ser cerrado por el usuario)
- Proteccion contra desinstalacion del agente
- Log de todas las operaciones financieras
- Backup automatico de la base de datos (diario)

---

## 10. Requisitos del Sistema

### Servidor (PC Admin):
- Windows 10/11
- .NET 8 Runtime
- 4 GB RAM minimo
- Red LAN configurada

### Clientes (PCs del cafe):
- Windows 10/11
- .NET 8 Runtime
- Conexion a la red LAN del servidor
