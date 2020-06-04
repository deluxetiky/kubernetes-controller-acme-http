# Let's Encrypt Acme Solver Multi Domain Kubernetes Controller With JetStack

When you have multiple domains that are in progress of renewing ssl certificate at the same time and you are not using the reverse proxy which has no auto tls renewing feature then you are not alone!

This project creates acme challenge verification api dynamicly based on the challange crds which are cert-manager.io/v1alpha2 custom definitions.

It is not a generic repository to handle all cases but it aims to provide some sample code blocks for the kubernetes controller with c#.

## Run


## Config


## Build

`docker build -t <name> -f ./acme-resolver/Dockerfile .`