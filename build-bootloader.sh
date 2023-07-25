#!/bin/bash
docker run --mount "type=bind,src=$(pwd)/Bootloader,dst=/ScreepsDotNetBootloader" -w /ScreepsDotNetBootloader --rm node npm run installandbuild
