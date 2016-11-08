import React, {Component, PropTypes} from "react";
import GridSystem from "dnn-grid-system";
import GridCell from "dnn-grid-cell";
import Label from "dnn-label";
import localization from "../../localization";
import ThemeSelector from "./ThemeSelector/ThemeSelector";

class Appearance extends Component {

    render() {        
        return (
            <GridSystem>
                <GridCell columnSize={100}>
                    <ThemeSelector />
                </GridCell>
                <GridCell columnSize={100}>
                    <Label 
                        label={localization.get("Layout")} 
                        tooltipMessage={localization.get("AddTooltipHere_TODO")} />
                </GridCell>
                <GridCell columnSize={100}>
                    <Label 
                        label={localization.get("PageContainer")} 
                        tooltipMessage={localization.get("AddTooltipHere_TODO")} />
                </GridCell>
            </GridSystem>
        );
    }
}

Appearance.propTypes = {
    page: PropTypes.object.isRequired
};

export default Appearance;
