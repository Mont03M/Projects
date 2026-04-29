FROM node:24-alpine
WORKDIR /client
COPY package*.json ./
RUN npm install
COPY . .
ENV CHOKIDAR_USEPOLLING=true
EXPOSE 3000
CMD ["npm", "start"]