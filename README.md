# AirWolfVendorFix
Respawns missing Airwolf vehicle vendor, which can turn up missing for some reason.

An admin-level player simply goes to the desired location and types /vsp.

New with 1.0.2: /bsp command for missing boat vendor
New with 1.0.6: /hsp command for missing horse vendor

NOTE: These vendors are designed to work in conjunction with known and perhaps fixed spawn locations in the game ONLY.

## Commands
  -- /vsp - Spawn an Airwolf mini vendor.
  -- /bsp - Spawn a boat vendor.
  -- /hsp - Spawn a horse vendor.

## Configuration
```json
{
  "Options": {
    "autoPlaceVendors": false,
    "alwaysPlaceVendors": false,
    "placeBoatVendor": true,
    "placeMiniVendor": true,
    "placeHorseVendor": true,
    "fixInvisibleBanditVendors": true,
    "debug": false
  },
  "Version": {
    "Major": 1,
    "Minor": 0,
    "Patch": 6
  }
}
```

If "autoPlaceVendors" is true, then the Airwolf vendor will be re-created on plugin load.  However, if "alwaysPlaceVendors" is false, it will only be placed if the vendor is not already there.  If that variable is true, it will be killed and re-created each time.

If "placeMiniVendor" is false, none of the re-create automation will happen.  This is there as a remnant and for future placement of fishing village boat vendors, which currently consist of at least 3 different known layouts.

