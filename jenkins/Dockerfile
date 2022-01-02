FROM jenkins/jenkins:2.327-jdk11
ENV JAVA_OPTS -Djenkins.install.runSetupWizard=false
ENV CASC_JENKINS_CONFIG /var/jenkins_home/casc.yaml
RUN jenkins-plugin-cli --plugins configuration-as-code:1.55
RUN jenkins-plugin-cli --plugins job-dsl:1.78.1
RUN jenkins-plugin-cli --plugins workflow-job:1145.v7f2433caa07f
RUN jenkins-plugin-cli --plugins workflow-scm-step:2.13
RUN jenkins-plugin-cli --plugins git:4.10.1
RUN jenkins-plugin-cli --plugins workflow-aggregator
RUN jenkins-plugin-cli --plugins xunit:3.0.5
RUN jenkins-plugin-cli --plugins jaxb:2.3.0.1
RUN jenkins-plugin-cli --plugins ws-cleanup:0.40
COPY casc.yaml /var/jenkins_home/casc.yaml
USER root
RUN groupadd --gid 984 docker
RUN apt-get update && apt-get install -y docker.io
RUN usermod -a -G docker jenkins
USER jenkins