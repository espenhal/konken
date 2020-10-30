# konken

### Docker
##### Run locally
docker run -d -p 8090:80 konken

##### Build image
docker build . -t konken

##### and push to repository / registry

docker tag konken:latest espenhal/konken:latest
docker login docker.io
docker push espenhal/konken:latest
