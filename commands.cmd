docker run --name smtp -d -p 3000:80 -p 2525:25 rnwood/smtp4dev
docker run --name seq -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 datalust/seq:latest

# how to log all requests fron nginx
# check defaults users

