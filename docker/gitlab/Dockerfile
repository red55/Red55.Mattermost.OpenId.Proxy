FROM gitlab/gitlab-ce:latest

ENV DEBIAN_FRONTEND=noninteractive

RUN curl -sfL https://github.com/devcontainers-contrib/nanolayer/releases/download/v0.5.6/nanolayer-x86_64-unknown-linux-gnu.tgz | tar fxvz - -C /tmp/ \
    && chmod 755 /tmp/nanolayer \
    && /tmp/nanolayer install apt-get mc,tshark \
    && rm /tmp/nanolayer

