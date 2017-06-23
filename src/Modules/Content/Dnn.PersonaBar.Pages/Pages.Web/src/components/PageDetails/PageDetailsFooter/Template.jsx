import React, { Component, PropTypes } from "react";
import Label from "dnn-label";
import Localization from "../../../localization";
import Dropdown from "dnn-dropdown";


const styles = {
    label: {
        width: "100%",
        paddingBottom: "10px"
    },
    dropdown: {
        width: "95%"
    }
};

class Template extends Component {
    getTemplatesOptions() {
        const {props} = this;
        const {templates} = props;
        return templates.map(template => {
            return {value: template.Value, label: template.Id};
        });
    }
    
    render() {
        const {selectedTemplateId, templates, onSelect} = this.props;
        
        if (!templates || templates.length === 0) {
            return null;
        }
        
        return <div>
            <Label style={styles.label}
                labelType="inline"
                tooltipMessage={Localization.get("TemplateTooltip")}
                label={Localization.get("Template")}
                />
            <Dropdown style={styles.dropdown} 
                options={this.getTemplatesOptions()}
                value={selectedTemplateId}
                onSelect={onSelect.bind(this)} 
                withBorder={true} />
        </div>;
    }
}

Template.propTypes = {
    templates: PropTypes.arrayOf(PropTypes.object).isRequired,
    selectedTemplateId: PropTypes.number.isRequired,
    onSelect: PropTypes.func.isRequired
};

export default Template;