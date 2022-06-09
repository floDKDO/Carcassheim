Avoir le programme du serveur qui tourne auto :

[Lien vers le guide](https://swimburger.net/blog/dotnet/how-to-run-a-dotnet-core-console-app-as-a-service-using-systemd-on-linux)

Etapes à reproduire :

Sélectionner le bon utilisateur et se déplacer dans le bon répertoire :
```
su - pro
cd ../ubuntu/pi_carcassonne/Appli_serveur_test
```

Extraire la "Release" :
```
sudo mkdir /srv/Appli_serveur_test               
sudo chown yourusername /srv/Appli_serveur_test 
sudo dotnet publish -c Release -o /srv/Appli_serveur_test/
```

Vérifier que l'extraction a réussie :
```
/srv/Appli_serveur_test/Appli_serveur_test
```

Créer le fichier "Appli_serveur_test.service" :
```
sudo nano Appli_serveur_test.service
```

et y entrer les informations suivantes :
```
[Unit]
Description=Appli_serveur_test console application

[Service]
# systemd will run this executable to start the service
# if /usr/bin/dotnet doesn't work, use `which dotnet` to find correct dotnet executable path
ExecStart=/usr/bin/dotnet /srv/Appli_serveur_test/Appli_serveur_test.dll
# to query logs using journalctl, set a logical name here
SyslogIdentifier=Appli_serveur_test

# Use your username to keep things simple.
# If you pick a different user, make sure dotnet and all permissions are set correctly to run the app
# To update permissions, use 'chown yourusername -R /srv/HelloWorld' to take ownership of the folder and files,
#       Use 'chmod +x /srv/HelloWorld/HelloWorld' to allow execution of the executable file
User=pro

# ensure the service restarts after crashing
Restart=always
# amount of time to wait before restarting the service                        
RestartSec=5 

# This environment variable is necessary when dotnet isn't loaded for the specified user.
# To figure out this value, run 'env | grep DOTNET_ROOT' when dotnet has been loaded into your shell.
Environment=DOTNET_ROOT=/usr/lib64/dotnet

[Install]
WantedBy=multi-user.target
```

Déplacer le fichier au bon endroit :
```
sudo cp Appli_serveur_test.service /etc/systemd/system/Appli_serveur_test.service
sudo systemctl daemon-reload
sudo systemctl start Appli_serveur_test
```

Pour vérifier l'état du service, deux versions :
```
sudo systemctl status Appli_serveur_test
sudo journalctl -u Appli_serveur_test
```

Pour vider le journal :
```
sudo journalctl --rotate
sudo journalctl --vacuum-time=1s
```

Après avoir apporté des modifications dans le code (depuis le bon répertoire!!!):
```
sudo systemctl stop Appli_serveur_test
sudo dotnet publish -c Release -o /srv/Appli_serveur_test
sudo systemctl start Appli_serveur_test
```

Après avoir apporté des modifications dans le fichier de service (depuis le bon répertoire!!!):
```
sudo cp Appli_serveur_test.service /etc/systemd/system/Appli_serveur_test.service
sudo systemctl daemon-reload
sudo systemctl restart Appli_serveur_test
```

Autoriser le redémarrage automatique lors du reboot du serveur
```
sudo systemctl enable Appli_serveur_test
```

