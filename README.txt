Both WebAPI's have versioning enabled, so to perfom any acton is a must 
change the URL like "{ip-address:port}/api/v1.0/{controller}"

WebAPI2 consumes WebAPI1,
To work properly just change in "appsettings.json"  the key "BaseURL" with the correct WebAPI1 IP-Address
