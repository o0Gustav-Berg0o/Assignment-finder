FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:1.27-alpine AS runtime
COPY --from=build /app/dist/assignment-finder-ui/browser /usr/share/nginx/html
COPY nginx.conf.template /etc/nginx/templates/default.conf.template

# The base image's entrypoint renders every /etc/nginx/templates/*.template through envsubst
# (restricted to currently-set env vars, so nginx's own $host/$uri/etc. are left untouched) into
# /etc/nginx/conf.d/ on container start - that's how API_INTERNAL_HOST reaches nginx.conf.template.
ENV API_INTERNAL_HOST=api:8080

EXPOSE 80
