# VEFramework Blockchain Indexer Server

Tento server provádí indexaci blockchainu, tak aby bylo možné získat seznam použitelných Utxo pro nové transakce. Také dokáže zprostředkovat informace o transakcích, blocích a adresách a umožňuje i poslání transakce do blockchainové sítě.

Aktuální verze serveru je vyvinutá pro Neblio blockchain, ale je celkem jednoduché ji upravit pro jakýkoliv Utxo blockchain a věřím, že není složité udělat verzi i pro Account based blockchainy.

## Požadavky na spuštění

Hlavní je mít nainstalovaný .NET 7.0 pro daný OS (minimálně Runtime). Konkrétní verze je možné stáhnout [zde](https://dotnet.microsoft.com/en-us/download/dotnet/7.0). Pokud budete chtít mít vždy jednoduše nejnovější verzi, je možné si naklonovat repozitář avšak v tu chvíli potřebujete .NET 7.0 . Aktuální verze skriptu, který stáhne .NET 7.0 SDK, dále naklonuje repozitář a vypublikuje aplikaci a spustí ji, je [zde](https://github.com/fyziktom/VirtualEconomyFramework/blob/182-blockchain-indexer/VirtualEconomyFramework/VEFramework.BlockchainIndexerServer/Resources/vef-bis-installation-script.sh).

V tuto chvíli byl server vyzkoušen na Windows 11 a Ubuntu 18.04 ve VM v Azure a také jako docker.

## Konfigurace

Hlavní konfigurace se nachází v souboru `appsettings.json`. V něm je možné nastavit následující parametry:

- BaseAddress : Jedná se o doménu na které poběží server
- APIAddress : Jedná se o stejnou adresu na které poběží server, nicméně lze využít i jinou adresu. Toto je API adresa pro VEDrivesLite, aby se věděly, kde ptát na tx info, apod.
- NumberOfBlocksInHistory : Číslo říká, kolik se nahraje při inicializaci bloků zpět do historie, než se začne plný běh serveru
- OldestBlockToLoad : Číslo nejstaršího bloku, který chci indexovat automatickou rutinou při běhu serveru. Pokud mám nový projekt a nezajímají mě třeba bloky před blokem 1000000, tak nastavím tento parametr na 1000000 a na tomto bloku se zastaví automatické donačítání historie v pozadí.
- StartFromTheLatestBlock : True znamená, že server začne indexovat pozpátku, tedy od nejnovějšího bloku. Díky tomu mají uživatelé i při resetu možnost co nejdříve začít API používat a najít použitelná Utxa.
- QTRPC : Nastavení pro připojení k RPC blockchain nodu.
  - Host": adresa RPC nodu (většinou localhost)
  - Port : port RPC nodu (u neblio 6326)
  - User": uživatelské jméno pro RPC připojení pokud je nastaveno na serveru
  - Pass : heslo pro RPC připojení pokud je nastaveno na serveru
  - SSL : pokud je doména na https tak je potřeba nastavit zde "true"
 
Kromě těchto parametrů je ještě potřeba zajistit, aby měl blockchain node spuštěný RPC server. Defaultně to nebývá a nebo s heslem, které je potřeba zjistit. Je tedy bezpečnější nastavit vlastní konfiguraci. To se provede ve složce, kde jsou uložené soubory nodu. V Linuxu je to například v:

```bash
cd /home/USERNAME/.neblio/
```

Pokud pouštíte node pod rootem, tak můžou být soubory v 

```bash
cd /root/.neblio/
```

Pokud v této složce ještě není vytvořený soubor neblio.conf (což lze ověřit pomocí ```bash ls -a ```), tak jej vytvoříme pomocí příkazu:

```bash
nano neblio.conf
```

V tomto souboru je potřeba doplnit:

```text
server=1
rpcallowip=*
rpcuser=user
rpcpassword=pass
rpcport=6326
```

Příkaz "rpcallowip=*" znamená, že má přístup jakákoliv adresa, která zná jméno a heslo. Takže je vhodné toto nastavení použít jen pokud je node zakrytý pomocí proxy. Pokud je node přístupný přímo z internetu, tak je vhodné nastavit konkrétní adresy, které mají přístup.

Po provedení úprav je potřeba restartovat node, aby se změny projevily. Je nutné poznamenat, že většina nodů po startu musí projít blockchain data a provést další startovací procedury, takže může někdy zabrat až 15 minut než node začne plně komunikovat na RPC.

## Spuštění služeb na Linuxu

Pokud potřebujete spustit server v pozadí jako službu, tak je možné využít následující postup:

 Přesuňte se do složky služeb:

```bash
cd /etc/systemd/system/
```

Zde vytvořte nový soubor s názvem nebliod.service:
```bash
sudo nano nebliod.service
```

Do tohoto souboru vložte následující obsah:

```text
[Unit]
Description=Neblio blockchain node
After=network.target

[Service]
WorkingDirectory=/home/YOUR_USERNAME/.neblio/
ExecStart=/usr/local/bin/nebliod
Restart=always
RestartSec=10
SyslogIdentifier=nebliod
User=YOUR_USERNAME

[Install]
WantedBy=multi-user.target

```

Nezapomeňte vyplnit své uživatelské jméno v názvu složek i u parametru User! Složky musí odkazovat na umístění, kde máte "dotnet" a aplikaci.

Jako další je potřeba vytvořit službu pro indexer server:

Zde vytvořte nový soubor s názvem blindexer.service:
```bash
sudo nano blindexer.service
```

Do tohoto souboru vložte následující obsah:

```text
[Unit]
Description=My Blazor Server App
After=network.target

[Service]
WorkingDirectory=/home/YOUR_USERNAME/vef/VirtualEconomyFramework/VirtualEconomyFramework/VEFramework.BlockchainIndexerServer/publish
ExecStart=/home/YOUR_USERNAME/dotnet/dotnet /home/YOUR_USERNAME/vef/VirtualEconomyFramework/VirtualEconomyFramework/VEFramework.BlockchainIndexerServer/publish/VEFramework.BlockchainIndexerServer.dll
Restart=always
RestartSec=10
SyslogIdentifier=blindexer
User=YOUR_USERNAME

[Install]
WantedBy=multi-user.target
```

Nezapomeňte vyplnit své uživatelské jméno v názvu složek i u parametru User! Složky musí odkazovat na umístění, kde máte "dotnet" a aplikaci.

Po uložení souboru (v nano je ukončení pomocí Ctrl+X, pak Y pro uložení změn a enter pro potvrzení) je potřeba spustit následující příkazy:
```bash
sudo systemctl daemon-reload
sudo systemctl enable nebliod.service
sudo systemctl enable blindexer.service
sudo systemctl start nebliod.service
sudo systemctl start blindexer.service
```

Službu blindexer je lepší zkusit až po spuštění nodu, protože indexer server se snaží připojit na RPC server. To že už RPC server komunikuje je možné zjistit například tímto příkazem:

Prvně si vytvoříme base64 coded string s heslem a jménem pro připojení:

```bash
echo Username:Password | Base64
```

To pak vložíme do příkazu:
```bash
curl -X POST "http://127.0.0.1:6326/" -H  "accept: application/json" -H  "Content-Type: application/json" -H "Authorization: Basic USERNAMEANDPASSBASE64" -d "{\"jsonrpc\":\"1.0\",\"id\":\"neblio-apis\",\"method\":\"getstakinginfo\",\"params\":[]}"
```

Zakódované jméno a heslo přijde namísto "USERNAMEANDPASSBASE64". Pokud je vše v pořádku, tak by měl přijít, který vypadá asi takto:

```json
{"result":{"enabled":true,"staking":false,"staking-criteria":{"mature-coins":false,"wallet-unlocked":true,"online":true,"synced":true},"errors":"","stakableoutputs":0,"currentblocksize":1000,"currentblocktx":0,"pooledtx":0,"difficulty":0.00261287,"search-interval":0,"weight":0,"netstakeweight":366094,"expectedtime":-1},"error":null,"id":"neblio-apis"}
```

Pokud ne, tak je potřeba zkontrolovat, zda je RPC server spuštěný a zda je správně nastavený.

Spolu nodem a serverem je vhodné spustit ještě IPFS node. Ten poskytne jednoduché uložení větších souborů. Pro něj bude potřeba vytvořit službu ipfs.service:

Prvně je potřeba nainstalovat IPFS

```bash
wget https://dist.ipfs.tech/kubo/v0.20.0/kubo_v0.20.0_linux-amd64.tar.gz
tar -xvzf kubo_v0.20.0_linux-amd64.tar.gz
cd kubo
sudo bash install.sh
ipfs --version
```

IPFS je poté potřeba inicializovat a také nastavit limity na uložiště a povolit http gateway pro přístup k souborům:

```bash
ipfs config Addresses.Gateway /ip4/0.0.0.0/tcp/8080
ipfs config Datastore.StorageMax "10GB"
ipfs config Datastore.StorageGCWatermark 90
```

První příkaz je spíš pro jistotu. Definuje adresu na které běží webserver gateway pro IPFS. Gateway by měla být defaultně zapnutá, ale je lepší mít jistotu.
Parametr "10GB" můžete změnit na svou velikost kterou chcete vyhradit pro IPFS. Následný příkaz definuje úroveň procent zaplnění daného alokovaného místa při kterém se pustí IPFS garbage collector, který odstraní bloky, které nejsou připnuté na daném nodu.

Zde vytvořte nový soubor s názvem ipfs.service:
```bash
sudo nano ipfs.service
```

Do tohoto souboru vložte následující obsah:

```text
[Unit]
Description=InterPlanetary File System (IPFS) daemon
After=network.target

[Service]
TimeoutStartSec=infinity
Type=notify
User=YOUR_USERNAME
StateDirectory=ipfs
Environment=IPFS_PATH=/home/YOUR_USERNAME/.ipfs/
ExecStart=/usr/local/bin/ipfs daemon
Restart=on-failure

[Install]
WantedBy=default.target

```


Poté je potřeba opět reloadnout systemctl daemon, povolit službu a spustit ji:

```bash
sudo systemctl daemon-reload
sudo systemctl enable ipfs.service
sudo systemctl start ipfs.service
```

Pokud chcete spustit službu jen v konzoli a ne jako službu, tak lze zavolat:

```bash
sudo ipfs daemon
```


Tím by měly být nastavené tři základní služby, které je dobré mít spolu na VM, tedy Blockchain node, IPFS node a Blockchain indexer server. Aby bylo možné ke službám přistupovat bezpečně, je vhodné ještě spustit a nastavit proxy. Já zde používám nginx.

Prvně je potřeba jej nainstalovat:

```bash
sudo apt-get install nginx
sudo systemctl start nginx
```

nginx už při instalaci by měl být doplněn do služeb v pozadí, takže stačí ho jen spustit.

Pro následující krok je potřeba mít vytvořený certifikát pro doménu, kterou chcete použít. Certifikát lze získat například pomocí Let's Encrypt a certbot:

```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d your-domain.com -d www.your-domain.com

```

kde "your-domain.com" je název Vaší domény. Tento skript by měl automaticky nastavit nxing aby již používal novou doménu.

Následně v nastavení nginxu je potřeba zeditovat soubor:

```bash
sudo nano /etc/nginx/sites-available/default
```

Obsah je trochu komplikovanější, takže ho popíšu postupně. Celý soubor je [zde](https://github.com/fyziktom/VirtualEconomyFramework/blob/182-blockchain-indexer/VirtualEconomyFramework/VEFramework.BlockchainIndexerServer/Resources/nginx.conf).

Na začátku souboru je definice limitů pro počet requestů na API:

```text
# limit number of connections per IP
limit_req_zone $binary_remote_addr zone=bis_api_limit:10m rate=500r/m;
limit_req_zone $binary_remote_addr zone=ipfs_upload_limit:10m rate=20r/m;
```

Konkrétně zde jsou dva limity, protože jiný je na klasické požadavky typu GetTransactionInfo a jiný zásadně menší na počet requestů na upload souborů.

V další části je již definice bloku server.

```text
server {...

    # listen on HTTPS
    listen 443 ssl;
    listen [::]:443 ssl;

    # Add certificates for ssl for the domain
    ssl_certificate /etc/letsencrypt/live/YOUR_DOMAIN_NAME/fullchain.pem; # managed by Certbot
    ssl_certificate_key /etc/letsencrypt/live/YOUR_DOMAIN_NAME/privkey.pem; # managed by Certbot

    server_name YOUR_DOMAIN;

    client_max_body_size 50M;
```

tato část říká, že bude server naslouchat na portu 443 a používat ssl. Odkazy na certifikáty by měl doplnit Certbot automaticky. Pokud není vyplněné server_name tak vyplňte název domény. Poslední příkaz definuje maximální velikost těla requestu. To je důležité pro limitování velikosti uploadovaných souborů. Zde je to například na 50 MB.

V další části je již oblsuha konkrétních requestů:

```text
    # redirect Neblio node RPC requests
    # if server is under domain with ssl it needs to be here attached to some location in this server
    location /node {
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP 127.0.0.1;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;	   
        proxy_pass http://127.0.0.1:6326/;
        proxy_cache_bypass $http_upgrade;
        allow all;
    }
```

Jako první je přesměrování requestů na Neblio node. Toto je potřeba pouze pokud je server pod doménou s SSL. V tom případě je potřeba přesměrovat requesty na Neblio node na jiný port, protože Neblio node běží na jiném portu než 443. V tomto případě je to port 6326. Pokud je server bez SSL, tak je toto přesměrování zbytečné.

Další location řeší upload IPFS souborů přes API:

```text
    # IPFS POST Command for upload item with API
    location /api/v0/add {
        limit_req zone=ipfs_upload_limit burst=50 nodelay;
        if ($request_method = 'OPTIONS') {
	            add_header 'Access-Control-Allow-Origin' '*' always;
	            #add_header 'Access-Control-Allow-Credentials' 'true' always;
	            add_header 'Access-Control-Allow-Headers' 'Accept, Keep-Alive,  Origin, X-Requested-With, Content-Range, X-Chunked-Output, X-Stream-Output, Content-Type' always;
	            add_header 'Access-Control-Allow-Methods' 'GET, POST, OPTIONS';
	            add_header 'Access-Control-Max-Age' 1728000;
	            add_header 'Content-Type' 'text/plain; charset=utf-8';
	            add_header 'Content-Length' 0;
	            return 204;
	    }
	    
            add_header 'Access-Control-Allow-Origin' '*' always;
            add_header 'Access-Control-Allow-Credentials' 'true' always;
            add_header 'Access-Control-Allow-Headers' 'Accept, Keep-Alive, Origin, X-Requested-With, Content-Range, X-Chunked-Output, X-Stream-Output, Content-Type' always;
	        add_header 'Access-Control-Allow-Methods' 'GET, POST, OPTIONS';
               
	        proxy_pass http://localhost:5001;
	        proxy_hide_header 'Access-Control-Allow-Origin';
            proxy_set_header Host $host;
            proxy_cache_bypass $http_upgrade;
            allow all;
	    
    }
```

Toto nastavení je složitější protože obsahuje i zpracování OPTIONS requestu, který chodí v případě, kdy je API voláno z nějaké webové aplikace na webserveru. Na OPTIONS příkaz je potřeba správně odpovědět, povolit správné příkazy a headery a až v druhém kole řešit přesměrování na službu.

Podobné je to i pro další příkazy IPFS API: 

```text
    # IPFS POST Command for obtain item with API
    location ~ ^/api/v0/(cat|pin/ls|pin/rm) {
	if ($request_method = 'OPTIONS') {
            add_header 'Access-Control-Allow-Origin' '*' always;
            add_header 'Access-Control-Allow-Credentials' 'true' always;
            add_header 'Access-Control-Allow-Methods' 'GET, POST, OPTIONS';
            add_header 'Access-Control-Allow-Headers' 'Accept ,X-CustomHeader,Keep-Alive,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type';
            add_header 'Access-Control-Max-Age' 1728000;
            add_header 'Content-Type' 'text/plain charset=UTF-8';
            add_header 'Content-Length' 0;
            return 204;
        }

        proxy_pass http://localhost:5001;
        proxy_hide_header 'Access-Control-Allow-Origin';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        allow all;
    }
```

U ostatních příkazů už je možné je spojit dohromady což je vidět v location, kde je regulární výraz pro rozpoznání 3 typů příkazů.

Další location řeší přesměrování IPFS gateway requestů:

```text
    # IPFS GET commands
    location /ipfs {
        add_header 'Access-Control-Allow-Origin' '*' always;

        if ($request_method = 'OPTIONS') {
		    add_header 'Access-Control-Allow-Credentials' 'true' always;
		    add_header 'Access-Control-Allow-Headers' 'Accept, Keep-Alive,  Origin, X-Requested-With, Content-Range, X-Chunked-Output, X-Stream-Output, Content-Type' always;
		    add_header 'Access-Control-Allow-Methods' 'GET, POST, OPTIONS';
		    add_header 'Access-Control-Max-Age' 1728000;
		    add_header 'Content-Type' 'text/plain; charset=utf-8';
		    add_header 'Content-Length' 0;
            return 204;
        }

	    add_header 'Access-Control-Allow-Credentials' 'true' always;
	    add_header 'Access-Control-Allow-Headers' 'Accept, Keep-Alive,  Origin, X-Requested-With, Content-Range, X-Chunked-Output, X-Stream-Output, Content-Type' always;
        add_header 'Access-Control-Allow-Methods' 'GET, POST, OPTIONS';

	    proxy_pass http://YOUR_SERVER_IP:8080;
	    proxy_hide_header 'Access-Control-Allow-Origin';
        proxy_hide_header 'Origin';
	    proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
```

Nyní už se dostáváme k sekci Blockchain Indexer Serveru. První location řeší POST příkazy:

```text
    # commands for the BlockchainIndexerServer API
	# POST commands
    location ~* ^/api/(BroadcastTransaction|CreateNotSignedTokenTransaction|CreateNotSignedIssueTransaction) {
            if ($request_method = 'OPTIONS') {
		    add_header 'Access-Control-Allow-Origin' '*' always;
		    add_header 'Access-Control-Allow-Credentials' 'true' always;
		    add_header 'Access-Control-Allow-Headers' 'Accept, Keep-Alive,  Origin, X-Requested-With, Content-Range, X-Chunked-Output, X-Stream-Output, Content-Type' always;
		    add_header 'Access-Control-Allow-Methods' 'GET, POST, OPTIONS';
		    add_header 'Access-Control-Max-Age' 1728000;
		    add_header 'Content-Type' 'text/plain; charset=utf-8';
		    add_header 'Content-Length' 0;
		    return 204;
	    }
	
	    add_header 'Access-Control-Allow-Origin' '*' always;
	    add_header 'Access-Control-Allow-Credentials' 'true' always;
	    add_header 'Access-Control-Allow-Headers' 'Accept, Keep-Alive, Origin, X-Requested-With, Content-Range, X-Chunked-Output, X-Stream-Output, Content-Type' always;
	    add_header 'Access-Control-Allow-Methods' 'GET, POST, OPTIONS';
			   
	    proxy_pass http://localhost:5000;
	    proxy_hide_header 'Access-Control-Allow-Origin';
	    proxy_set_header Host $host;
	    proxy_cache_bypass $http_upgrade;
	    allow all;

    }
```

Před posledním location, které je pro ostatní příkazy je ještě dobré vložit sekci pro ChatGPT plugin. Tam je potřeba správně odkázat na [openapi.yaml](https://github.com/fyziktom/VirtualEconomyFramework/blob/182-blockchain-indexer/VirtualEconomyFramework/VEFramework.BlockchainIndexerServer/wwwroot/openapi.yaml) a logo.png. Ostatní soubory jsou ve složce wwwroot/.well-known.


```text
    root /home/tomas/vef/VirtualEconomyFramework/VirtualEconomyFramework/VEFramework.BlockchainIndexerServer/publish/wwwroot;

    location = /openapi.yaml {
        try_files $uri =404;
    }
    location = /logo.png {
        try_files $uri =404;
    }
```

Poté je již možné nadefinovat obecný / location:

```text
    # other GET commands
    location / {
        limit_req zone=bis_api_limit burst=2000 nodelay;

        add_header 'Access-Control-Allow-Origin' '*' always;

        if ($request_method = 'OPTIONS') {
            add_header 'Access-Control-Allow-Origin' '$http_origin' always;
            add_header 'Access-Control-Allow-Methods' 'GET, OPTIONS, POST, PUT' always;
            return 204;
        }
          
        proxy_pass http://localhost:5000;
        proxy_hide_header 'Access-Control-Allow-Origin';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
```

Na konci souboru bude ještě přidané od certbot toto:

```text

    # process path for the ssl certificates certbot generator
    location ^~ /.well-known/acme-challenge {
        allow all;
        root /var/www/certbot;
    }
```

a toto za definicí hlavního serveru:

```text
# Certbot
server {
    if ($host = ve-framework.com) {
        return 301 https://$host$request_uri;
    } # managed by Certbot

    listen 80 default_server;
    listen [::]:80 default_server;

    server_name ve-framework.com;
    return 404; # managed by Certbot
}
```

Po dokončení konfigurace je potřeba uložit soubor, restartovat nginx.

```bash
sudo systemctl restart nginx
```

Vhodné je mít při nastavování po ruce ChatGPT4, protože dokáže s těmito nastaveními v Linuxu velmi dobře pomoci.
