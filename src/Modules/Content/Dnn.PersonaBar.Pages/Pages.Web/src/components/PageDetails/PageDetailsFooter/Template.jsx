import React, { Component, PropTypes } from "react";
import Label from "dnn-label";
import Localization from "../../../localization";
import Dropdown from "dnn-dropdown";


const styles = {
    label: {
        width: "100%"
    },
    dropdown: {
        width: "95%"
    }
};

class Template extends Component {
    getTemplatesOptions(){
        const {props} = this;
        const {templates} = props;
        return templates.map(template => {
            return {value: template.Value, label: template.Id};
        });
    }
    
    onSelect(option) {
        
    }
    
    render() {
        return <div>
            <Label style={styles.label}
                labelType="inline"
                tooltipMessage={Localization.get("TemplateTooltip")}
                label={Localization.get("Template")}
                />
            <Dropdown style={styles.dropdown} 
                options={this.getTemplatesOptions()}
                value="" 
                onSelect={this.onSelect.bind(this)} 
                withBorder={true} />
        </div>;
    }
}

Template.propTypes = {
    templates: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default Template;