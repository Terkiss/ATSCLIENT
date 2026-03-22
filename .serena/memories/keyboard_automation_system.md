# MapleATS Keyboard Automation System

## Architecture
- `KeyCommandData`: Data model for a single keyboard command.
  - `Id`: Unique integer ID.
  - `Key`: Key name (e.g., "A", "Enter").
  - `Delay`: Pre-action delay in milliseconds.
  - `Action`: "on" (KeyDown) or "off" (KeyUp or Stroke).
- `KeyBoard`: Low-level key input using `keybd_event` (Win32 API).
  - Handles `on` (KeyDown) and `off` (KeyUp if pressed, else Down-Delay-Up).
  - Random delay for stroke: 50ms ~ 250ms.
- `KeyCommandGenerate`: High-level management.
  - `Dictionary<int, KeyCommandData> CommandRepository`: Storage by ID.
  - `HashSet<int> PressedCommandIds`: Tracks currently "on" keys.
  - `AddCommand(string)`: Parses `<Key>,sleep,<Delay>,<Action>`.
  - `RemoveCommand(int)`: Removes from storage and releases key if pressed.
  - `ExecuteCommand(int)`: Executes and updates pressed state.

## CLI Commands
- `add <Key>,sleep,<Delay>,<Action>`
- `rem <Id>`
- `run <Id>`
- `list`
