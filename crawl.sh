#!/bin/bash

# Usage: ./crawl.sh {table_name} {url} {depth}

table_name=$1
url=$2
depth=$3

aws dynamodb put-item --table-name $table_name --item '{ "crawlerurl": {"S": "'$url'"}, "crawlerdepth": {"N": "'$depth'" } }' --return-consumed-capacity TOTAL



