pipeline {
    agent any
    stages {
        stage('clean') {
            steps {
                cleanWs()
            }
        }
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        stage('in docker') {
            steps {
                sh 'bash tarbikmap build'
            }
        }
    }
    post {
        always {
            xunit (
                tools: [ MSTest(pattern: 'outputs/**/unit_tests.xml') ]
            )
            archiveArtifacts artifacts: 'outputs/**'
        }
    }
}