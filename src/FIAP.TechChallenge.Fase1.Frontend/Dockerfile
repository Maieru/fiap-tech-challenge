FROM node:22-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm ci --no-audit --no-fund

COPY . .

RUN npm run build

FROM nginx:1.27-alpine AS final
ENV OS_UPSTREAM=http://host.docker.internal:8081 \
    EXECUTION_UPSTREAM=http://host.docker.internal:8082 \
    BILLING_UPSTREAM=http://host.docker.internal:8083 \
    OTEL_EXPORTER_OTLP_ENDPOINT=http://host.docker.internal:4318
WORKDIR /usr/share/nginx/html

COPY nginx.conf /etc/nginx/templates/default.conf.template
COPY --from=build /app/dist .

EXPOSE 80
