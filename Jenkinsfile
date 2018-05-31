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

            }
        }

        stage('Checkout') {
            steps {

                parallel (
                    "DNNConnect.CKEditorProvider": {
                        // Check out DNNConnect.CKEditorProvider
                        checkout([$class: 'GitSCM',
                            branches: [[name: "${env.sha1}"]],
                            doGenerateSubmoduleConfigurations: false,
                            extensions: [
                                [$class: 'CleanBeforeCheckout'],
                                [$class: 'LocalBranch', localBranch: "${env.ghprbSourceBranch}"],
                                [$class: 'RelativeTargetDirectory', relativeTargetDir: "${env.RELCKEditorCheckout}"],
                            ],
                            submoduleCfg: [],
                            userRemoteConfigs: [[
                                credentialsId: 'github-access',
                                url: 'git@github.com:trilogy-group/CKEditorProvider-Security.git',
                                permissions: 'WRITABLE',
                                refspec: '+refs/pull/*:refs/remotes/origin/pr/*'
                            ]]
                        ])
                    },

                    "DNN.Platform": {

                        // Check out DNN.Platform
                        checkout([$class: 'GitSCM',
                            branches: [[name: "${env.platformBranch}"]],
                            doGenerateSubmoduleConfigurations: false,
                            extensions: [
                                [$class: 'CleanBeforeCheckout'],
                                [$class: 'SubmoduleOption', recursive: false, parentCredentials: true],
                                [$class: 'CloneOption', noTags: true],
                                [$class: 'LocalBranch', localBranch: "${env.platformBranch}"],
                                [$class: 'RelativeTargetDirectory', relativeTargetDir: "${env.RELPlatformCheckout}"],
                                [$class: 'SparseCheckoutPaths',  sparseCheckoutPaths:[
                                        [$class:'SparseCheckoutPath', path:'/DNN Platform/Website/Install']
                                ]
                                ]
                            ],
                            submoduleCfg: [],
                            userRemoteConfigs: [[
                                credentialsId: 'github-access',
                                url: 'git@github.com:dnnsoftware/Dnn.Platform.git',
                                permissions: 'WRITABLE'
                            ]]
                        ])
                    }
                )
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

    }

    post {
        success {

               // Copy to Platform, commit, and push to GitHub
                bat """
mkdir "${env.RELPlatformCheckout}\\DNN Platform\\Website\\Install\\Provider"
del /Q "${env.RELPlatformCheckout}\\DNN Platform\\Website\\Install\\Provider\\DNNConnect.CKEditorProvider_*.zip"
copy /Y "${env.RELCKEditorCheckout}\\packages\\*.zip" "${env.RELPlatformCheckout}\\DNN Platform\\Website\\Install\\Provider\\"

pushd ${env.RELPlatformCheckout}
git add -f "DNN Platform\\Website\\Install\\Provider\\DNNConnect.CKEditorProvider_*.zip"
git commit -am "CI/CD agent auto-commit of CKEditor build #${env.BUILD_NUMBER} for Platform ${env.PlatformVersion}"

git pull --rebase origin ${env.platformBranch}
git push origin HEAD

popd
"""
        }
        always {
            updateGithubCommitStatus(
                status: currentBuild.currentResult,
                repositoryPath: env.RELPlatformCheckout
            )
        }
    }
}