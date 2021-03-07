#!/bin/sh

branch=`git branch --show-current`
pipeline=test-pipeline.yml
if [ "$branch" = "main" ]; then
    pipeline=deploy-pipeline.yml
fi
fly --target $1 set-pipeline -c $pipeline -p $branch-pipeline --team css432 -v branch=$branch
