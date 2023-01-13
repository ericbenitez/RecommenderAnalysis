import { plot } from 'nodeplotlib';
import * as readFileSync from 'fs';
// const plot = require('nodeplotlib').plot;
// const readFileSync = require('fs').readFileSync;

const fileString = readFileSync("Experiment_Output.txt", 'utf8');

let experiments = []; // List of all experiments
let experimentMatrix = [];
let experimentList = fileString.split("-----");
for(let experiment of experimentList) {
    let experimentObj = {};
    let experimentInfo = experiment.split(",");
    experiment.name = experimentInfo[0].split("=")[1];
    experiment.negatives = experimentInfo[1].split("=")[1];
    experiment.algorithm = experimentInfo[2].split("=")[1];
    experiment.size = experimentInfo[3].split("=")[1];
    experiment.method = experimentInfo[4].split("=")[1];
    experiment.threshold = experimentInfo[5].split("=")[1];
    experiment.microseconds = experimentInfo[6].split("=")[1];
    experiment.mae = experimentInfo[7].split("=")[1];
    experiments.push(experiment);
    
    let row = [];
    row.push(experiment.name); //0
    row.push(experiment.negatives); //1
    row.push(experiment.algorithm); //2
    row.push(experiment.size); //3
    row.push(experiment.method); //4
    row.push(experiment.threshold); //5
    row.push(experiment.microseconds); //6
    row.push(experiment.mae); //7
    experimentMatrix.push(row);
}

//Plot MAE of method 1 across all neighbourhood sizes for test.txt
let namedTest = filterRows(experimentMatrix, 0, "test");
let methodOne = filterRows(namedTest, 0, "test");
let mae = getColumn(methodOne, 7);
let neighbourhoodSizes = getColumn(methodOne, 3);


const test1NeighbourhoodSizeMAE = [
    {
        x: neighbourhoodSizes,
        y: mae,
        type: 'scatter',
    }
]

plot(test1NeighbourhoodSizeMAE)

function filterRows(matrix ,attribute, threshold){
    matrix.filter(function(row) {return row[attribute]===threshold;});
}

function getColumn(matrix, columnNum){
    let column = matrix.map(function(row) { return row[columnNum]; });
    return column;
}

// const plotData = [
//     {
//         x: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
//         y: [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
//         type: 'scatter',
//     }
// ]

// plot(plotData);

exports = {

}