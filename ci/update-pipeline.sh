#!/bin/sh

branch=`git branch --show-current`
fly --target $1 set-pipeline -c test-pipeline.yml -p $branch-pipeline --team css432 -v branch=$branch
