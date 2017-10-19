"use strict";

Object.defineProperty(exports, "__esModule", {
    value: true
});

var _createClass = function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; }();

var _react = require("react");

var _react2 = _interopRequireDefault(_react);

var _dnnTooltip = require("dnn-tooltip");

var _dnnTooltip2 = _interopRequireDefault(_dnnTooltip);

var _dnnMultiLineInput = require("dnn-multi-line-input");

var _dnnMultiLineInput2 = _interopRequireDefault(_dnnMultiLineInput);

var _dnnLabel = require("dnn-label");

var _dnnLabel2 = _interopRequireDefault(_dnnLabel);

require("./style.less");

function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { default: obj }; }

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _possibleConstructorReturn(self, call) { if (!self) { throw new ReferenceError("this hasn't been initialised - super() hasn't been called"); } return call && (typeof call === "object" || typeof call === "function") ? call : self; }

function _inherits(subClass, superClass) { if (typeof superClass !== "function" && superClass !== null) { throw new TypeError("Super expression must either be null or a function, not " + typeof superClass); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, enumerable: false, writable: true, configurable: true } }); if (superClass) Object.setPrototypeOf ? Object.setPrototypeOf(subClass, superClass) : subClass.__proto__ = superClass; }

var MultiLineInputWithError = function (_Component) {
    _inherits(MultiLineInputWithError, _Component);

    function MultiLineInputWithError() {
        _classCallCheck(this, MultiLineInputWithError);

        var _this = _possibleConstructorReturn(this, (MultiLineInputWithError.__proto__ || Object.getPrototypeOf(MultiLineInputWithError)).call(this));

        _this.state = {
            isFocused: false
        };
        return _this;
    }

    _createClass(MultiLineInputWithError, [{
        key: "onBlur",
        value: function onBlur(e) {
            var props = this.props;

            if (props.hasOwnProperty("onBlur")) {
                props.onBlur(e);
            }
            this.setState({ isFocused: false });
        }
    }, {
        key: "onFocus",
        value: function onFocus(e) {
            var props = this.props;

            if (props.hasOwnProperty("onFocus")) {
                props.onFocus(e);
            }
            this.setState({ isFocused: true });
        }
    }, {
        key: "getClass",
        value: function getClass() {
            var props = this.props;

            var errorClass = props.error ? " " + props.errorSeverity : "";
            var enabledClass = props.enabled ? "" : " disabled";
            var customClass = " " + props.className;
            return "dnn-multi-line-input-with-error" + errorClass + customClass + enabledClass;
        }
    }, {
        key: "getCounter",
        value: function getCounter(counter) {
            if (!this.shouldRenderCounter(counter)) {
                return null;
            }

            return _react2.default.createElement(
                "div",
                { className: "dnn-inline-counter" },
                counter
            );
        }
    }, {
        key: "shouldRenderCounter",
        value: function shouldRenderCounter(counter) {
            var counterIsDefined = !!counter || counter === 0;
            return this.state.isFocused && counterIsDefined;
        }
    }, {
        key: "getInputRightPadding",
        value: function getInputRightPadding(counter, error) {

            var padding = 0;
            if (counter || counter === 0) {
                padding += 10 + counter.toString().length * 8;
            }
            if (error) {
                padding += 22;
            }

            return padding;
        }
    }, {
        key: "render",
        value: function render() {
            var props = this.props;

            var errorMessages = props.errorMessage instanceof Array ? props.errorMessage : [props.errorMessage];
            return _react2.default.createElement(
                "div",
                { className: this.getClass(), style: props.style },
                props.label && _react2.default.createElement(_dnnLabel2.default, {
                    labelFor: props.inputId,
                    label: props.label,
                    tooltipMessage: props.tooltipMessage,
                    tooltipPlace: props.infoTooltipPlace,
                    tooltipActive: props.tooltipMessage,
                    labelType: props.labelType,
                    className: props.infoTooltipClassName,
                    style: Object.assign(!props.tooltipMessage ? { marginBottom: 5 } : {}, props.labelStyle)
                }),
                props.extraToolTips,
                _react2.default.createElement(
                    "div",
                    { className: "input-tooltip-container " + props.labelType },
                    _react2.default.createElement(_dnnMultiLineInput2.default, {
                        id: props.inputId,
                        onChange: props.onChange,
                        onBlur: this.onBlur.bind(this),
                        onFocus: this.onFocus.bind(this),
                        onKeyDown: props.onKeyDown,
                        onKeyPress: props.onKeyPress,
                        onKeyUp: props.onKeyUp,
                        value: props.value,
                        tabIndex: props.tabIndex,
                        style: Object.assign({ marginBottom: 32, paddingRight: this.getInputRightPadding(props.counter, props.error) }, props.inputStyle),
                        placeholder: props.placeholder,
                        enabled: props.enabled,
                        maxLength: props.maxLength
                    }),
                    this.getCounter(props.counter),
                    _react2.default.createElement(_dnnTooltip2.default, {
                        messages: errorMessages,
                        type: props.errorSeverity,
                        className: props.placement,
                        tooltipPlace: props.tooltipPlace,
                        rendered: props.error })
                )
            );
        }
    }]);

    return MultiLineInputWithError;
}(_react.Component);

MultiLineInputWithError.propTypes = {
    inputId: _react.PropTypes.string,
    label: _react.PropTypes.string,
    infoTooltipClassName: _react.PropTypes.string,
    tooltipMessage: _react.PropTypes.oneOfType([_react.PropTypes.string, _react.PropTypes.array]),
    infoTooltipPlace: _react.PropTypes.string,
    labelType: _react.PropTypes.string,
    className: _react.PropTypes.string,
    inputSize: _react.PropTypes.oneOf(["large", "small"]),
    error: _react.PropTypes.bool,
    errorMessage: _react.PropTypes.oneOfType([_react.PropTypes.string, _react.PropTypes.array]),
    errorSeverity: _react.PropTypes.oneOf(["error", "warning"]),
    counter: _react.PropTypes.number,
    tooltipPlace: _react.PropTypes.string,
    placement: _react.PropTypes.oneOf(["outside", "inside"]),
    onChange: _react.PropTypes.func,
    onBlur: _react.PropTypes.func,
    onFocus: _react.PropTypes.func,
    onKeyDown: _react.PropTypes.func,
    onKeyPress: _react.PropTypes.func,
    onKeyUp: _react.PropTypes.func,
    value: _react.PropTypes.any,
    enabled: _react.PropTypes.bool,
    tabIndex: _react.PropTypes.number,
    inputStyle: _react.PropTypes.object,
    placeholder: _react.PropTypes.string,
    style: _react.PropTypes.object,
    labelStyle: _react.PropTypes.object,
    extraToolTips: _react.PropTypes.node,
    maxLength: _react.PropTypes.number
};
MultiLineInputWithError.defaultProps = {
    error: false,
    enabled: true,
    className: "",
    placement: "inside",
    labelType: "block",
    errorMessage: ["This field has an error."],
    errorSeverity: "error"
};
exports.default = MultiLineInputWithError;
//# sourceMappingURL=MultiLineInputWithError.js.map