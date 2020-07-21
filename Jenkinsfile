pipeline {
  agent {
    docker {
      image 'mcr.microsoft.com/dotnet/core/sdk'
      reuseNode true
    }

  }
  stages {
    stage('检出') {
      steps {
        checkout([
          $class: 'GitSCM',
          branches: [[name: env.GIT_BUILD_REF]],
          userRemoteConfigs: [[
            url: env.GIT_REPO_URL,
            credentialsId: env.CREDENTIALS_ID
          ]]])
        }
      }
      stage('构建与测试') {
        steps {
          sh 'dotnet test --logger:"junit;LogFileName=test-result.xml" --collect:"XPlat Code Coverage" --settings \'./Timeline.Tests/coverletArgs.runsettings\''
          junit 'Timeline.Tests/TestResults/test-result.xml'
        }
      }
    }
    environment {
      ASPNETCORE_ENVIRONMENT = 'Development'
    }
  }