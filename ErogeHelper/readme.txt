+-------------+
|   Program   |
+-------------+
  |
  | game path
  v
+-------------+                           +--------------+
| AppLauncher | .......................>  | ProcessStart |
+-------------+    optional funciton      +--------------+
  |
  |
  v
Start ErogeHelper.AssistiveTouch.exe

+---------+
| IpcMain | --> wait for touch first show
+---------+

          +------------------+
   ---->  |   AppLauncher    | <---
   |      +------------------+    |
   |                              |
   | dependent on                 |
   |                              |
+------------------+  +------------------+
|   PEFileReader   |  | RegistryModifier |
+------------------+  +------------------+
