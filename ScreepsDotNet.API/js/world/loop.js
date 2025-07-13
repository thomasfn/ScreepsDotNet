if (!startupComplete) {
    startupComplete = startup();
    return;
}
if (startupComplete) {
    bootloader.loop();
}
