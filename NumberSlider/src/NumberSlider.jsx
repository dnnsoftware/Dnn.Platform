import React, {Component, PropTypes} from "react";
import ReactSlider from "react-slider";
import "./style.less";

export default class NumberSlider extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        const minimum = this.props.min;
        const maximum = this.props.max;
        let step = this.props.step || 1;

        const value = this.props.value;

        const className = "slider-container" + (this.props.withMinMax ? " with-min-max": "");
        
        return (
            <div className={className}>
                {this.props.withMinMax && <span>{minimum}</span>}
                <ReactSlider 
                    min={minimum} 
                    value={value}
                    max={maximum}
                    step={step}
                    onChange={this.props.onChange}
                    disabled={!!this.props.disabled}
                    withBars={true}>
                        {!this.props.hideValue && <div>{value}</div>}
                    </ReactSlider>
                    
                {this.props.withMinMax && <span>{maximum}</span>}
            </div>
        );
    }
}

NumberSlider.propTypes = {
    min: PropTypes.number.isRequired, 
    max: PropTypes.number.isRequired,
    step: PropTypes.number.isRequired,
    value: PropTypes.number.isRequired,
    onChange: PropTypes.func.isRequired,
    disabled: PropTypes.bool,
    withMinMax: PropTypes.bool,
    hideValue: PropTypes.bool
};