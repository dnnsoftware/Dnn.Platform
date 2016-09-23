import React, {PropTypes, Component} from "react";
import GridCell from "dnn-grid-cell";
import "./style.less";

class GridSystem extends Component {
    constructor() {
        super();
        this.uniqueId = "Grid-" + (Date.now() * Math.random());
    }
    getStyle() {
        const {props} = this;
        return Object.assign({ width: (props.width || 100) + (props.type ? props.type : "%") }, props.style);
    }
    /*
        getColumns() method.
        This method calculates the width of each column based on the number of columns specified.
        If no number is specified, then the width is calculated via the length of the children inside.
     */
    getColumns() {
        const {props} = this;
        let children = [];
        for (let i = 0; i < (props.numberOfColumns || props.children.length); i++) {
            children.push(<GridCell key={this.uniqueId + i} columnSize={100 / (props.numberOfColumns || props.children.length)} style={props.gridCellStyle}>{props.children[i]}</GridCell>);
        }
        return children;
    }
    render() {
        const {props} = this;
        return (
            <div className={"dnn-grid-system " + props.className}  style={this.getStyle() }>
                {this.getColumns() }
            </div>
        );
    }
}

GridSystem.PropTypes = {
    children: PropTypes.node,
    width: PropTypes.number,
    type: PropTypes.string,
    numberOfColumns: PropTypes.number,
    gridCellStyle: PropTypes.object,
    style: PropTypes.object,
    className: PropTypes.string
};

GridSystem.defaultProps = {
    className: ""
};

export default GridSystem;