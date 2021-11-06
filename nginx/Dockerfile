FROM --platform=linux nginx:1.19.8 AS build-env
WORKDIR /cert

COPY openssl.cnf /etc/ssl/openssl.cnf

RUN openssl req -x509 -newkey rsa:4096 -sha256 -days 3650 -nodes -keyout /cert/tarbikmap.key -out /cert/tarbikmap.crt -subj "/CN=tarbikmap.local" -addext "subjectAltName=DNS:tarbikmap.local,DNS:www.tarbikmap.local"

FROM --platform=linux nginx:1.19.8
RUN rm /etc/nginx/conf.d/default.conf
RUN cat /etc/nginx/nginx.conf
COPY default.conf.template /etc/nginx/templates/default.conf.template
COPY --from=build-env /cert/tarbikmap.crt /etc/ssl/certs/tarbikmap.crt
COPY --from=build-env /cert/tarbikmap.key /etc/ssl/private/tarbikmap.key
