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
          sh 'dotnet test --logger:"html;LogFileName=index.html" --collect:"XPlat Code Coverage" --settings \'./Timeline.Tests/coverletArgs.runsettings\''
          codingHtmlReport(name: 'test-result', path: 'Timeline.Tests/TestResults/', entryFile: 'index.html')
        }
      }
    }
    environment {
      ASPNETCORE_ENVIRONMENT = 'Development'
    }
  }