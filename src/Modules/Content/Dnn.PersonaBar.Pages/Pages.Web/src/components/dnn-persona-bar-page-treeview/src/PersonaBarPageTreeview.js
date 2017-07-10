import React, {Component} from "react";
import GridCell from "dnn-grid-cell";

export class PersonaBarPageTreeview extends Component {
    renderCollapseExpand() {
        return (
            <span>
                [COLLAPSE EXPAND]
            </span>
        );
    }

    render() {
        return (
            <GridCell columnSize={30}  style={{marginTop:"120px", backgroundColor:"#aaa"}} >
                {this.renderCollapseExpand()}
            </GridCell>
        );
    }

}

