FROM mcr.microsoft.com/devcontainers/typescript-node:1-22

WORKDIR /workspace/frontend

COPY package*.json ./
RUN npm install

COPY . .

EXPOSE 3000
