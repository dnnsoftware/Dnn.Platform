#!/usr/bin/env groovy
@Library(['trilogy-group/dnn-jenkins-pipelines', 'eng-build-utils']) _
currentBuild.displayName =  "#${env.currentVersion}.${env.BUILD_ID}"
println "BUILD_NUMBER set to ${currentBuild.displayName}"


pipeline {
    agent { label 'dnn' }
    options { timestamps(); buildDiscarder(logRotator(numToKeepStr: '10', artifactNumToKeepStr: '10')) }

    environment {
        msbuild = "%ProgramFiles(x86)%/MSBuild/14.0/Bin/MSBuild.exe"

        BUILD_NUMBER = "${env.currentVersion}.${env.BUILD_ID}"
        RELPlatformCheckout = "DNN.Platform"
        RELCKEditorCheckout = "CKEditor"
        checkoutDirectory = "${env.WORKSPACE}"
        CKEditorCheckout = "${env.WORKSPACE}/${env.RELCKEditorCheckout}"
        PlatformCheckout = "${env.WORKSPACE}/${env.RELPlatformCheckout}"
    }

    stages {

        stage('Environment') {
            steps { 
                bat "set" 
                
                dir(env.RELPlatformCheckout) {
                    deleteDir()
                }
            }
        }

        stage('Checkout DNN.Platform') {
            steps {
                // Check out DNN.Platform
                dir(env.RELPlatformCheckout) {

                    // we don't use
                    // if we use it, Jenkins triggers this jobs when DNN.Platform reposiroy updated
                    sshagent(['github-access']) {
                        bat """
git init
git config core.sparseCheckout true
git remote add -f origin git@github.com:dnnsoftware/Dnn.Platform.git
SET SPARSE_CONFIG=/DNN Platform/Website/Install
echo %SPARSE_CONFIG% > .git/info/sparse-checkout
git checkout ${env.platformBranch}
"""
                    }
                }
            }
        }

        stage('Build') {
            steps {
                dir("${env.RELCKEditorCheckout}") {
                    // Nuget Restore
                    bat "nuget restore DNNConnect.CKEditorProvider.sln -Source https://api.nuget.org/v3/index.json -Source https://www.myget.org/F/dnn-software-public/api/v3/index.json"

                    // Nuget Update
                    bat "nuget update DNNConnect.CKEditorProvider.sln -Safe -RepositoryPath \"packages\" -Source https://api.nuget.org/v3/index.json -Source https://www.myget.org/F/dnn-software-public/api/v3/index.json"

                    // Rebuild using MSBuild
                    bat "\"${env.msbuild}\" DNNConnect.CKEditorProvider.sln /t:Rebuild /v:m /nologo /m /p:Platform=\"Any CPU\" /p:Configuration=Release /p:DebugSymbols=true /p:DebugType=pdbonly"
                }
            }
        }

        stage('Commit&Push To DNN.Platform') {
            when { not { environment name: 'buildType', value: 'CI' } }
            steps {
                sshagent(['github-access']) {
                    // Copy to Platform, commit, and push to GitHub
                    bat """
mkdir "${env.RELPlatformCheckout}\\DNN Platform\\Website\\Install\\Provider"
del /Q "${env.RELPlatformCheckout}\\DNN Platform\\Website\\Install\\Provider\\DNNConnect.CKEditorProvider_*.zip"
copy /Y "${env.RELCKEditorCheckout}\\packages\\*.zip" "${env.RELPlatformCheckout}\\DNN Platform\\Website\\Install\\Provider\\"

pushd ${env.RELPlatformCheckout}
git add -f "DNN Platform\\Website\\Install\\Provider\\DNNConnect.CKEditorProvider_*.zip"
git commit -am "CI/CD agent auto-commit of CKEditor build #${env.BUILD_NUMBER} for Platform ${env.PlatformVersion} [ci skip]"

git pull --rebase origin ${env.platformBranch}
git push origin HEAD

popd
"""
                }
            }
        }
    }

    post {
        always {
            updateGithubCommitStatus(
                status: currentBuild.currentResult,
                repositoryPath: env.RELPlatformCheckout
            )
        }
    }
}