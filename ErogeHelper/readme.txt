+-------------+
|   Program   |
+-------------+
  |
  | game path
  v
+-------------+              +---------+
| AppLauncher |     .......> | IpcMain | --> wait for touch first show
+-------------+     :        +---------+
  |                 :        +--------------+
  | .......................> | ProcessStart |
  |    optional funciton     +--------------+
  v
Start ErogeHelper.AssistiveTouch.exe





          +------------------+
   ---->  |   AppLauncher    | <---
   |      +------------------+    |
   |                              |
   | dependent on                 |
   |                              |
+------------------+  +------------------+
|   PEFileReader   |  | RegistryModifier |
+------------------+  +------------------+
