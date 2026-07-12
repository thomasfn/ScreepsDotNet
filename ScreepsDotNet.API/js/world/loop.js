if (!startupComplete) {
    startupComplete = startup();
}
if (startupComplete) {
    bootloader.loop();
}
