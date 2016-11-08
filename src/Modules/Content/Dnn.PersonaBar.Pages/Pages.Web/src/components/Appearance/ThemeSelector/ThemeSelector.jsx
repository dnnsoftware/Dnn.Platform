import React, {Component, PropTypes} from "react";
import Label from "dnn-label";
import localization from "../../../localization";
import { Scrollbars } from "react-custom-scrollbars";
import GridCell from "dnn-grid-cell";

class ThemeSelector extends Component {

    render() {        
        return (
            <div>
                <Label 
                    label={localization.get("PageTheme")} 
                    tooltipMessage={localization.get("PageThemeTooltip")} />
                <div>
                    <Scrollbars
                        className="theme-list"
                        autoHeight
                        autoHeightMin={0}
                        autoHeightMax={480}>
                        <GridCell>
                           
                           
                        </GridCell>
                    </Scrollbars>
                </div>
            </div>
        );
    }
}

ThemeSelector.propTypes = {
    currentTheme: PropTypes.object.isRequired,
    themes: PropTypes.array.isRequired
};

export default ThemeSelector;