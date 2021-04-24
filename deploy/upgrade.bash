docker pull crupest/timeline:latest
sudo systemctl stop timeline.service
docker rm timeline
docker create -v $HOME/timeline:/root/timeline -p 5000:80 --name timeline crupest/timeline:latest
sudo systemctl restart timeline.service
docker system prune
