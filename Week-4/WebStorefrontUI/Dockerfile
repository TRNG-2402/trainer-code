# Our first image, is huge. For no reason. Over half a gig in size
# And we can absolutely shrink that with a multi-stage build. 
# So, we can have one stage for building out outpuit bundle (for React, the /dist directory)
# And then toss out everything we needed software and dependency wise to get to that point,
# keeping /dist and the bare minimum software we need to run our webserver with our react app

# ------ Stage 1: Build Stage
FROM node:20-alpine AS build

# Next, we give our Dockerfile a Working Directory on our container to do its work in
WORKDIR /app
# Now any commands we execute in the dockerfile will be targeting/happening inside of 
# The ./app directory of the CONTAINER's file system. 

# Copy lets us move stuff from OUR file system, into the container's file system.
# The final argument (if you choose to copy multiple items at once) is the CONTAINER file system
# destination for what you're trying to copy over
COPY package.json package-lock.json ./
RUN npm ci

# Once we have our dependencies installed... we can then copy over our code
COPY . . 

# After we copy everything over, but before we run npm run build - we can bring over that
# VITE_API_BASE_URL env secret (or any other secrets we may need, like DB_CONN_STRINGS in the API)
ARG VITE_API_BASE_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL


RUN npm run build

# Stage 2: Runtime 
FROM nginx:alpine AS runtime

# Using nginx to serve our web page - much like sites have done for 
# nginx serves static files out of /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html


EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]