import { plot, Plot } from 'nodeplotlib';
import * as fs from 'fs';

type Experiment = {
    name: string;
    negatives: string;
    algorithm: string;
    size: string;
    method: string;
    threshold: string;
    microseconds: string;
    mae: string;
}

// const plot = require('nodeplotlib').plot;
// const readFileSync = require('fs').readFileSync;

const fileString = fs.readFileSync("../Data/Experiment_Output.txt", 'utf8');

let experiments: Experiment[] = []; // List of all experiments
let experimentMatrix: any[] = [];
let experimentList = fileString.split("-----");


for(let experiment of experimentList) {
    let experimentObj = {} as Experiment;
    let experimentInfo = experiment.split(",");
    experimentObj.name = experimentInfo[0].split("=")[1];
    experimentObj.negatives = experimentInfo[1].split("=")[1];
    experimentObj.algorithm = experimentInfo[2].split("=")[1];
    experimentObj.size = experimentInfo[3].split("=")[1];
    experimentObj.method = experimentInfo[4].split("=")[1];
    experimentObj.threshold = experimentInfo[5].split("=")[1];
    experimentObj.microseconds = experimentInfo[6].split("=")[1];
    experimentObj.mae = experimentInfo[7].split("=")[1];
    experiments.push(experimentObj);
    
    let row: any[] = [];
    row.push(experimentObj.name); //0
    row.push(experimentObj.negatives); //1
    row.push(experimentObj.algorithm); //2
    row.push(experimentObj.size); //3
    row.push(experimentObj.method); //4
    row.push(experimentObj.threshold); //5
    row.push(experimentObj.microseconds); //6
    row.push(experimentObj.mae); //7
    experimentMatrix.push(row);
}

//Plot MAE of method 1 across all neighbourhood sizes for test.txt
let namedTest = filterRows(experimentMatrix, 0, "test");
let methodOne = filterRows(namedTest, 0, "test");
let mae = getColumn(methodOne, 7);
let neighbourhoodSizes = getColumn(methodOne, 3);


const test1NeighbourhoodSizeMAE: Plot[] = [
    {
        x: neighbourhoodSizes,
        y: mae,
        type: 'scatter',
    }
]

plot(test1NeighbourhoodSizeMAE)

function filterRows(matrix: any[], attribute: number, threshold: any){
    let result = matrix.filter(function(row) {return row[attribute]===threshold;});
    return result;
}

function getColumn(matrix: any[], columnNum: number){
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