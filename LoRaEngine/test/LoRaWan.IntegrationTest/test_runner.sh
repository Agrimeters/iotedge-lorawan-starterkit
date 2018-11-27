#!/bin/bash

TEST_RESULTS_PATH="./TestResults"

function fail {
  echo $1 >&2
  exit 1
}

trap 'fail "The test execution was aborted because a command exited with an error status code."' ERR


function retry {
  local n=1
  local max=3
  local delay=5
  while true; do
    "$@" && break || {
      if [[ $n -lt $max ]]; then
        ((n++))
        echo "Test failed. Attempt $n/$max:"
        sleep $delay;
      else
        fail "The test has failed after $n attempts."
      fi
    }
  done
}



if [ "$1" != "" ]; then
    TEST_RESULTS_PATH="$1"
fi


echo "ABPTest starting..."
retry dotnet test --filter ABPTest --logger trx --results-directory $TEST_RESULTS_PATH

echo "C2DMessageTest starting..."
retry dotnet test --filter C2DMessageTest --logger trx --results-directory $TEST_RESULTS_PATH

echo "OTAAJoinTest starting..."
retry dotnet test --filter OTAAJoinTest --logger trx --results-directory $TEST_RESULTS_PATH

echo "OTAATest starting..."
retry dotnet test --filter OTAATest --logger trx --results-directory $TEST_RESULTS_PATH

echo "SensorDecodingTest starting..."
retry dotnet test --filter SensorDecodingTest --logger trx --results-directory $TEST_RESULTS_PATH

echo "Done executing tests starting..."