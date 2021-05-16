## Supercell.Magic - Project
***Supercell.Magic*** is a Clash of Clans Server.
It was written by myself.
The goal of this server was to implement all the features of Clash of Clans and support millions of players.
Supercell.Magic uses dedicated threads and async operators. 
**Couchbase servers** & a **memory based** saving server will be used to save players.

## About us
I probably won't update the git. I don't have time with the studies to work on it anymore.
If you have any questions, you can contact me on discord (@Mimi8297#8726).
You can use it to create your own private server if you wish. The current version has some logic bugs.
Here is a site that uses this version: https://atrasis.net.
I offer partnerships that allow you to have your own private server. Prices are $350/month. 


SETUP:
1.You have to start a local webserver create a supercell folder and copy the files from the directory Supercell.Magic-master\www into the directory. You should be able to access http://127.0.0.1/supercell environment.json and see a config then.
When you will start the start.bat you will face another problem or two.
You need to provide the game assets for the server.
Download the Clash of Clans APK 9.256.20, use WinRAR or similar to open it.
There you will see an assets folder. Copy it into your webserver directory.
Also don't forget to set up a couchbase server and a redis server.
When you set up the couchbase server you either have to use the credentials given in the environment.json or you use your own credentials and replace them in the file.
Once you are logged in, head over to Buckets and create the following buckets:
magic-players, magic-admin, magic-alliances, magic-streams, magic-seasons

If you want to use docker compose for it, create a docker compose file where the start.bat is located and copy the code below.
I have used the Supercell.Magic-master\www directory as the main directory for the webserver.
To access the couchbase server: http://localhost:8091/
Supercell.Magic-master\docker-compose.yaml