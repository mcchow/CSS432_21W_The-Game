#!/bin/sh

branch=`git branch --show-current`
read -s -p "GitHub Token: " github_password
echo
fly --target $1 set-pipeline -c test-pipeline.yml -p $branch-pipeline -v github-pw=$github_password --team css432 -v branch=$branch
