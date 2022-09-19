#!/bin/bash
cd ../Release/
zip -r -9 "EvaFuel-$(date --universal +%F-%T).zip" "EvaFuel"
